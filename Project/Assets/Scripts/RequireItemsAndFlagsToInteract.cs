using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RequireItemsAndFlagsToInteract : MonoBehaviour
{
    [Header("이 아이템들을 모두 가지고 있어야 클릭 가능")]
    public List<string> requiredItems = new();

    [Header("이 플래그들을 모두 만족해야 클릭 가능")]
    public List<string> requiredFlags = new();

    [Header("이 플래그들 중 하나라도 켜져 있으면 클릭 불가")]
    public List<string> forbiddenFlags = new();

    [Header("요구조건이 하나도 없으면 기본을 잠금(false)으로 둘지")]
    public bool lockWhenNoRequirements = true;

    [Header("SearchableObject가 있고, 드랍테이블이 비면 클릭/호버도 막기")]
    public bool requireSearchableHasLoot = false;

    [Header("잠겨있을 때 버튼을 투명 처리하고 싶으면 CanvasGroup 사용")]
    public bool visuallyHideWhenLocked = false;

    private Button _btn;
    private CanvasGroup _group;
    private SearchableObject _searchable;

    private bool _initialized;
    private bool _hasEvaluated;
    private bool _lastOk;

    private Coroutine _co;

    private void Awake()
    {
        // 같은 오브젝트에 Button이 없고 자식에만 있는 경우 대비
        _btn = GetComponent<Button>();
        if (_btn == null) _btn = GetComponentInChildren<Button>(true);

        _group = GetComponent<CanvasGroup>();
        _searchable = GetComponent<SearchableObject>();

        // 시작 순간은 무조건 잠금
        Apply(false);
    }

    private void OnEnable()
    {
        // 맵을 “처음 들어올 때”도 무조건 잠금부터
        Apply(false);

        if (_co != null) StopCoroutine(_co);
        _co = StartCoroutine(InitWhenReady());
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

        if (_co != null) { StopCoroutine(_co); _co = null; }
    }

    private void Update()
    {
        if (!_initialized) return;
        Evaluate(); // 플래그 변화는 이벤트가 없어서 주기 체크
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
        // 요구조건이 하나도 없으면 실수 방지로 잠금
        if (lockWhenNoRequirements &&
            (requiredItems == null || requiredItems.Count == 0) &&
            (requiredFlags == null || requiredFlags.Count == 0) &&
            (forbiddenFlags == null || forbiddenFlags.Count == 0) &&
            !requireSearchableHasLoot)
            return false;

        if (GameManager.I == null || FlagStore.I == null) return false;

        // 아이템(모두 필요)
        foreach (var id in requiredItems)
            if (!GameManager.I.HasItem(id))
                return false;

        // 플래그(모두 필요)
        foreach (var f in requiredFlags)
            if (!FlagStore.I.Has(f))
                return false;

        // 금지 플래그(하나라도 켜져 있으면 불가)
        foreach (var f in forbiddenFlags)
            if (FlagStore.I.Has(f))
                return false;

        // 드랍테이블 비면 불가
        if (requireSearchableHasLoot)
        {
            if (_searchable == null) _searchable = GetComponent<SearchableObject>();
            if (_searchable != null && !_searchable.HasNonTrashRemaining())
                return false;
        }

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
