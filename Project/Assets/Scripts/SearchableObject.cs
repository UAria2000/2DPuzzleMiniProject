using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SearchableObject : MonoBehaviour
{
    [Header("디버그용 ID (각 탐색 오브젝트마다 고유 추천)")]
    public string objectId;

    [Header("탐색 시 소모 시간(분)")]
    public int searchCostMinutes = 10;

    [Header("이 오브젝트에서 쪽지 드랍 가능?")]
    public bool canDropNotes = true;

    [Header("이 오브젝트에서 1회성으로 나올 수 있는 아이템들(중복 X)")]
    public List<string> initialOneTimeItems = new();

    [Header("버튼(없으면 자동으로 GetComponent)")]
    public Button button;

    [Header("쪽지/아이템이 더 이상 안 나오면(쓰레기만 남으면) 호버/클릭 자체 막기")]
    public bool blockRaycastsWhenEmpty = true;

    // 런타임 상태
    private List<string> _remainingOneTimeItems;
    private bool _noteAlreadyDroppedHere;
    private CanvasGroup _cg;

    private enum ResultType { Trash, Note, Item }

    private struct Result
    {
        public ResultType type;
        public string itemId;
        public Result(ResultType t, string id = null) { type = t; itemId = id; }
    }

    private void Awake()
    {
        if (button == null) button = GetComponent<Button>();
        _cg = GetComponent<CanvasGroup>();

        if (initialOneTimeItems == null) initialOneTimeItems = new List<string>();
    }

    private void Start()
    {
        // 시작 때 interactable을 true로 강제하지 않는다!
        EnsureRuntimeList();
        RefreshLockIfEmpty();
    }

    private void OnEnable()
    {
        EnsureRuntimeList();
        RefreshLockIfEmpty();
    }

    private void EnsureRuntimeList()
    {
        if (initialOneTimeItems == null) initialOneTimeItems = new List<string>();

        if (_remainingOneTimeItems == null)
            _remainingOneTimeItems = new List<string>(initialOneTimeItems);

        if (blockRaycastsWhenEmpty && _cg == null)
            _cg = GetComponent<CanvasGroup>(); // 필요하면 나중에 Add
    }

    // “쓰레기 말고” 얻을 수 있는 게 남아있는지
    public bool HasNonTrashRemaining()
    {
        EnsureRuntimeList();

        bool hasItems = _remainingOneTimeItems.Count > 0;

        bool notePossible =
            canDropNotes &&
            !_noteAlreadyDroppedHere &&
            NotePoolManager.I != null &&
            NotePoolManager.I.RemainingCount > 0;

        return hasItems || notePossible;
    }

    // ==========================
    // RandomUniqueItemAssigner 지원용 API
    // ==========================
    public void AddInitialOneTimeItem(string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return;

        if (initialOneTimeItems == null) initialOneTimeItems = new List<string>();
        if (!initialOneTimeItems.Contains(itemId))
            initialOneTimeItems.Add(itemId);

        EnsureRuntimeList();
        if (!_remainingOneTimeItems.Contains(itemId))
            _remainingOneTimeItems.Add(itemId);

        RefreshLockIfEmpty();
    }

    public void RemoveInitialOneTimeItem(string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return;

        if (initialOneTimeItems != null)
            initialOneTimeItems.Remove(itemId);

        if (_remainingOneTimeItems != null)
            _remainingOneTimeItems.Remove(itemId);

        RefreshLockIfEmpty();
    }

    // 새 회차 시작 시 호출(재시작/타이틀 복귀 후 새 시작 등)
    public void ResetRunState()
    {
        if (initialOneTimeItems == null) initialOneTimeItems = new List<string>();

        _remainingOneTimeItems = new List<string>(initialOneTimeItems);
        _noteAlreadyDroppedHere = false;

        // 여기서 interactable true / SetActive(true) 강제 금지
        RefreshLockIfEmpty();
    }

    public void Search()
    {
        EnsureRuntimeList();

        if (TimeManager.I != null)
            TimeManager.I.Spend(searchCostMinutes);

        var results = new List<Result>();

        for (int i = 0; i < _remainingOneTimeItems.Count; i++)
            results.Add(new Result(ResultType.Item, _remainingOneTimeItems[i]));

        bool notePossible =
            canDropNotes &&
            !_noteAlreadyDroppedHere &&
            NotePoolManager.I != null &&
            NotePoolManager.I.RemainingCount > 0;

        if (notePossible)
            results.Add(new Result(ResultType.Note));

        results.Add(new Result(ResultType.Trash));

        int pick = Random.Range(0, results.Count);
        var chosen = results[pick];

        switch (chosen.type)
        {
            case ResultType.Trash:
                Debug.Log($"[SEARCH] {objectId}: 쓰레기(허탕)");
                break;

            case ResultType.Item:
                GiveItemOnce(chosen.itemId);
                break;

            case ResultType.Note:
                GiveNote();
                break;
        }

        RefreshLockIfEmpty();
    }

    private void GiveItemOnce(string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return;

        Debug.Log($"[SEARCH] {objectId}: 아이템 획득 {itemId}");

        if (GameManager.I != null)
            GameManager.I.AddItem(itemId);

        _remainingOneTimeItems.Remove(itemId);
    }

    private void GiveNote()
    {
        if (NotePoolManager.I == null) return;

        if (NotePoolManager.I.TryTakeRandomNote(out var noteId))
        {
            Debug.Log($"[SEARCH] {objectId}: 쪽지 획득 {noteId}");

            _noteAlreadyDroppedHere = true;

            if (GameManager.I != null)
                GameManager.I.AddItem(noteId);
        }
    }

    // 비었을 때만 클릭/호버 자체 차단
    private void RefreshLockIfEmpty()
    {
        EnsureRuntimeList();

        if (!HasNonTrashRemaining())
        {
            if (button != null)
                button.interactable = false;

            if (blockRaycastsWhenEmpty)
            {
                if (_cg == null) _cg = gameObject.AddComponent<CanvasGroup>();
                _cg.blocksRaycasts = false;
                _cg.interactable = false;
            }
        }
        else
        {
            // 여기서는 "다시 켜기"를 하지 않음 (조건 잠금 스크립트가 관리)
            if (blockRaycastsWhenEmpty && _cg != null)
            {
                _cg.blocksRaycasts = true;
                _cg.interactable = true;
            }
        }
    }
}
