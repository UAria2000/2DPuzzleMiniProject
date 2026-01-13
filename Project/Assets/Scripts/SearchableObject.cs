using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SearchableObject : MonoBehaviour
{
    [Header("각 탐색 오브젝트마다 고유 ID (필수 추천)")]
    public string objectId;

    [Header("탐색 시 소모 시간(분)")]
    public int searchCostMinutes = 10;

    [Header("이 오브젝트에서 쪽지 드랍 가능?")]
    public bool canDropNotes = true;

    [Header("이 오브젝트에서 1회성으로 나올 수 있는 아이템들(중복 X)")]
    public List<string> initialOneTimeItems = new();

    [Header("버튼(없으면 자동으로 찾음)")]
    public Button button;

    [Header("‘소진(더 이상 얻을 게 없음)’ 상태면 호버/클릭 자체 막기")]
    public bool blockRaycastsWhenExhausted = true;

    [Header("처음부터 비어있어도 1번은 탐색(허탕) 가능하게")]
    public bool allowEmptySearchOnce = true;

    [Header("버튼을 강제로 켜지 않고, 소진일 때만 끄기(권장)")]
    public bool onlyDisableWhenExhausted = true;

    private List<string> _remainingOneTimeItems;
    private bool _noteAlreadyDroppedHere;
    private bool _searchedOnce;
    private CanvasGroup _cg;

    private enum ResultType { Trash, Note, Item }
    private struct Result
    {
        public ResultType type;
        public string itemId;
        public Result(ResultType t, string id = null) { type = t; itemId = id; }
    }

    private string KeyPrefix
    {
        get
        {
            if (string.IsNullOrEmpty(objectId))
            {
                objectId = gameObject.name;
                Debug.LogWarning($"[SearchableObject] objectId 비어있음 -> '{objectId}'로 자동 설정됨");
            }
            return $"SO_{objectId}_";
        }
    }

    private string FlagSearched => KeyPrefix + "S";
    private string FlagNoteUsed => KeyPrefix + "N";
    private string FlagTaken(string itemId) => KeyPrefix + "T_" + itemId;

    private void Awake()
    {
        if (button == null) button = GetComponent<Button>();

        if (initialOneTimeItems == null) initialOneTimeItems = new List<string>();
        _cg = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        InitFromFlags();
        ApplyExhaustedLock();
    }

    private void Start()
    {
        InitFromFlags();
        ApplyExhaustedLock();
    }

    private void InitFromFlags()
    {
        if (initialOneTimeItems == null) initialOneTimeItems = new List<string>();

        // 런타임 목록 구성
        _remainingOneTimeItems = new List<string>(initialOneTimeItems);

        // FlagStore 상태 반영(맵 재진입/재활성화 대응)
        if (FlagStore.I != null)
        {
            _searchedOnce = FlagStore.I.Has(FlagSearched);
            _noteAlreadyDroppedHere = FlagStore.I.Has(FlagNoteUsed);

            // 이미 가져간 아이템 제거
            for (int i = _remainingOneTimeItems.Count - 1; i >= 0; i--)
            {
                var id = _remainingOneTimeItems[i];
                if (FlagStore.I.Has(FlagTaken(id)))
                    _remainingOneTimeItems.RemoveAt(i);
            }
        }
    }

    public void AddInitialOneTimeItem(string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return;

        if (initialOneTimeItems == null) initialOneTimeItems = new List<string>();
        if (!initialOneTimeItems.Contains(itemId))
            initialOneTimeItems.Add(itemId);

        if (_remainingOneTimeItems == null) _remainingOneTimeItems = new List<string>();
        if (!_remainingOneTimeItems.Contains(itemId))
            _remainingOneTimeItems.Add(itemId);

        ApplyExhaustedLock();
    }

    public void RemoveInitialOneTimeItem(string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return;

        initialOneTimeItems?.Remove(itemId);
        _remainingOneTimeItems?.Remove(itemId);

        ApplyExhaustedLock();
    }

    public void ResetRunState()
    {
        // 런 리셋 시 FlagStore.ResetAll()을 보통 같이 하므로 여기선 내부 상태만 초기화
        _searchedOnce = false;
        _noteAlreadyDroppedHere = false;
        _remainingOneTimeItems = new List<string>(initialOneTimeItems);

        ApplyExhaustedLock();
    }

    public bool HasNonTrashRemaining()
    {
        bool hasItems = _remainingOneTimeItems != null && _remainingOneTimeItems.Count > 0;

        bool notePossible =
            canDropNotes &&
            !_noteAlreadyDroppedHere &&
            NotePoolManager.I != null &&
            NotePoolManager.I.RemainingCount > 0;

        return hasItems || notePossible;
    }

    private bool IsExhausted()
    {
        // allowEmptySearchOnce = true면, “한 번도 탐색 안 했으면” 비어 있어도 소진 아님(허탕 1회 허용)
        if (allowEmptySearchOnce && !_searchedOnce) return false;

        return !HasNonTrashRemaining();
    }

    public void Search()
    {
        if (IsExhausted())
        {
            ApplyExhaustedLock();
            return;
        }

        if (TimeManager.I != null)
            TimeManager.I.Spend(searchCostMinutes);

        _searchedOnce = true;
        if (FlagStore.I != null) FlagStore.I.Set(FlagSearched);

        var results = new List<Result>();

        // 아이템
        if (_remainingOneTimeItems != null)
            foreach (var id in _remainingOneTimeItems)
                results.Add(new Result(ResultType.Item, id));

        // 쪽지
        bool notePossible =
            canDropNotes &&
            !_noteAlreadyDroppedHere &&
            NotePoolManager.I != null &&
            NotePoolManager.I.RemainingCount > 0;

        if (notePossible)
            results.Add(new Result(ResultType.Note));

        // 쓰레기
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

        ApplyExhaustedLock();
    }

    private void GiveItemOnce(string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return;

        if (GameManager.I != null)
            GameManager.I.AddItem(itemId);

        _remainingOneTimeItems?.Remove(itemId);

        if (FlagStore.I != null)
            FlagStore.I.Set(FlagTaken(itemId));
    }

    private void GiveNote()
    {
        if (NotePoolManager.I == null) return;

        if (NotePoolManager.I.TryTakeRandomNote(out var noteId))
        {
            _noteAlreadyDroppedHere = true;

            if (FlagStore.I != null)
                FlagStore.I.Set(FlagNoteUsed);

            if (GameManager.I != null)
                GameManager.I.AddItem(noteId);
        }
    }

    private void ApplyExhaustedLock()
    {
        bool exhausted = IsExhausted();

        // 버튼은 “소진이면 끄기”
        if (button != null)
        {
            if (exhausted) button.interactable = false;
            else if (!onlyDisableWhenExhausted) button.interactable = true;
        }

        // 호버/클릭 자체 차단
        if (blockRaycastsWhenExhausted)
        {
            if (_cg == null) _cg = GetComponent<CanvasGroup>();
            if (_cg == null) _cg = gameObject.AddComponent<CanvasGroup>();

            if (exhausted)
            {
                _cg.blocksRaycasts = false;
                _cg.interactable = false;
            }
            else
            {
                _cg.blocksRaycasts = true;
                _cg.interactable = true;
            }
        }
    }
}
