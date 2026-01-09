using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class RequireItemsAndFlagsToInteract : MonoBehaviour
{
    [Header("이 아이템들을 모두 가지고 있어야 클릭 가능")]
    public List<string> requiredItems = new();

    [Header("이 플래그들을 모두 만족해야 클릭 가능")]
    public List<string> requiredFlags = new();

    [Header("이 플래그들 중 하나라도 켜져 있으면 클릭 불가 (예: bridgeActivated)")]
    public List<string> forbiddenFlags = new();   //  추가

    [Header("요구조건이 하나도 없으면 기본을 잠금(false)으로 둘지")]
    public bool lockWhenNoRequirements = true;

    [Header("잠겨있을 때 버튼을 투명 처리하고 싶으면 CanvasGroup 사용")]
    public bool visuallyHideWhenLocked = false;

    private Button _btn;
    private CanvasGroup _group;

    private bool _initialized;
    private bool _hasEvaluated;
    private bool _lastOk;

    private void Awake()
    {
        _btn = GetComponent<Button>();
        _group = GetComponent<CanvasGroup>();

        // 첫 평가 전 기본 잠금
        Apply(false);
    }

    private void OnEnable()
    {
        StartCoroutine(InitWhenReady());
    }

    private IEnumerator InitWhenReady()
    {
        while (GameManager.I == null || FlagStore.I == null) yield return null;

        GameManager.I.InventoryChanged += Evaluate;
        _initialized = true;

        _hasEvaluated = false;
        Evaluate();
    }

    private void OnDisable()
    {
        if (GameManager.I != null)
            GameManager.I.InventoryChanged -= Evaluate;

        _initialized = false;
    }

    private void Update()
    {
        if (!_initialized) return;
        Evaluate(); // 단순하게 주기적 평가
    }

    public void Evaluate()
    {
        bool ok = AreAllMet();

        if (_hasEvaluated && ok == _lastOk) return;

        _hasEvaluated = true;
        _lastOk = ok;
        Apply(ok);
    }

    private bool AreAllMet()
    {
        // 요구조건이 하나도 없으면(실수 방지) 잠금 처리 옵션
        if (lockWhenNoRequirements &&
            (requiredItems == null || requiredItems.Count == 0) &&
            (requiredFlags == null || requiredFlags.Count == 0) &&
            (forbiddenFlags == null || forbiddenFlags.Count == 0))
        {
            return false;
        }

        // 아이템 조건(모두 필요)
        if (GameManager.I == null) return false;
        foreach (var id in requiredItems)
            if (!GameManager.I.HasItem(id))
                return false;

        // 플래그 조건(모두 필요)
        if (FlagStore.I == null) return false;
        foreach (var f in requiredFlags)
            if (!FlagStore.I.Has(f))
                return false;

        //  금지 플래그(하나라도 켜져 있으면 불가)
        foreach (var f in forbiddenFlags)
            if (FlagStore.I.Has(f))
                return false;

        return true;
    }

    private void Apply(bool ok)
    {
        if (_btn != null)
            _btn.interactable = ok;

        if (visuallyHideWhenLocked)
        {
            if (_group == null) _group = gameObject.AddComponent<CanvasGroup>();

            _group.alpha = ok ? 1f : 0f;
            _group.blocksRaycasts = ok;
            _group.interactable = ok;
        }
    }
}
