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

    [Header("요구조건이 하나도 없으면(리스트 비어있으면) 기본 잠금 처리")]
    public bool lockWhenNoRequirements = true;

    [Header("잠겨있을 때 버튼을 투명 처리하고 싶으면 CanvasGroup 사용")]
    public bool visuallyHideWhenLocked = false;

    [Header("잠겨있을 때 호버/클릭 자체도 막기(레이캐스트 차단)")]
    public bool blockRaycastsWhenLocked = true;

    [Header("SearchableObject가 붙어있다면, 드랍(쪽지/아이템)이 남아있을 때만 클릭 가능")]
    public bool requireSearchableHasLoot = true;

    private Button _btn;
    private CanvasGroup _group;
    private SearchableObject _searchable;

    private bool _initialized;
    private bool _hasEvaluated;
    private bool _lastOk;

    private void Awake()
    {
        _btn = GetComponent<Button>();
        _group = GetComponent<CanvasGroup>();
        _searchable = GetComponent<SearchableObject>();

        // 초기에는 잠금으로 두기(매니저 준비 전에도 클릭되는 것 방지)
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

    // 플래그는 이벤트가 없어서 주기 체크(간단 버전)
    private void Update()
    {
        if (!_initialized) return;
        Evaluate();
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
        // 요구조건이 아무것도 없으면 기본 잠금(실수 방지)
        if (lockWhenNoRequirements &&
            (requiredItems == null || requiredItems.Count == 0) &&
            (requiredFlags == null || requiredFlags.Count == 0) &&
            !(requireSearchableHasLoot && _searchable != null))
        {
            return false;
        }

        //  SearchableObject가 비었으면(쓰레기만 남으면) 무조건 잠금
        if (requireSearchableHasLoot && _searchable != null)
        {
            if (!_searchable.HasNonTrashRemaining())
                return false;
        }

        // 아이템 조건
        if (GameManager.I == null) return false;
        if (requiredItems != null)
        {
            foreach (var id in requiredItems)
                if (!GameManager.I.HasItem(id))
                    return false;
        }

        // 플래그 조건
        if (FlagStore.I == null) return false;
        if (requiredFlags != null)
        {
            foreach (var f in requiredFlags)
                if (!FlagStore.I.Has(f))
                    return false;
        }

        return true;
    }

    private void Apply(bool ok)
    {
        if (_btn != null) _btn.interactable = ok;

        // 레이캐스트 차단(호버/클릭 방지)
        if (blockRaycastsWhenLocked || visuallyHideWhenLocked)
        {
            if (_group == null) _group = gameObject.AddComponent<CanvasGroup>();

            if (blockRaycastsWhenLocked)
            {
                _group.blocksRaycasts = ok;
                _group.interactable = ok;
            }

            if (visuallyHideWhenLocked)
                _group.alpha = ok ? 1f : 0f;
        }
    }
}
