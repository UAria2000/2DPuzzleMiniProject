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

    [Header("드랍이 비었을 때(쓰레기만 남았을 때) 호버/클릭 자체를 막기")]
    public bool blockRaycastsWhenEmpty = true;

    // 런타임 상태
    private List<string> _remainingOneTimeItems;
    private bool _noteAlreadyDroppedHere;

    private CanvasGroup _cg;

    private enum ResultType { Trash, Note, Item }

    private struct Result
    {
        public ResultType type;
        public string itemId; // Note/Item일 때만 사용
        public Result(ResultType t, string id = null) { type = t; itemId = id; }
    }

    private void Awake()
    {
        if (button == null) button = GetComponent<Button>();
        _cg = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        ResetRunState();
    }

    private void OnEnable()
    {
        // 맵 패널이 꺼졌다 켜질 때도 상태 반영
        if (_remainingOneTimeItems == null)
            _remainingOneTimeItems = new List<string>(initialOneTimeItems);

        RefreshInteractable();
    }

    // ✅ “쓰레기 말고” 실제로 얻을 수 있는(쪽지/아이템) 것이 남아있는지
    public bool HasNonTrashRemaining()
    {
        int itemCount = (_remainingOneTimeItems != null) ? _remainingOneTimeItems.Count : initialOneTimeItems.Count;
        bool hasItems = itemCount > 0;

        bool notePossible =
            canDropNotes &&
            !_noteAlreadyDroppedHere &&
            NotePoolManager.I != null &&
            NotePoolManager.I.RemainingCount > 0;

        return hasItems || notePossible;
    }

    // 새 회차 시작 시 호출
    public void ResetRunState()
    {
        _remainingOneTimeItems = new List<string>(initialOneTimeItems);
        _noteAlreadyDroppedHere = false;

        if (button != null) button.interactable = true;

        if (blockRaycastsWhenEmpty)
        {
            if (_cg == null) _cg = gameObject.AddComponent<CanvasGroup>();
            _cg.blocksRaycasts = true;
            _cg.interactable = true;
        }

        gameObject.SetActive(true);
    }

    public void Search()
    {
        // 시간 소모
        if (TimeManager.I != null)
            TimeManager.I.Spend(searchCostMinutes);

        // 가능한 결과 목록 만들기
        var results = new List<Result>();

        // 1) 남아있는 1회성 아이템들
        if (_remainingOneTimeItems != null)
        {
            foreach (var id in _remainingOneTimeItems)
                results.Add(new Result(ResultType.Item, id));
        }

        // 2) 쪽지(조건 만족 시만)
        bool notePossible =
            canDropNotes &&
            !_noteAlreadyDroppedHere &&
            NotePoolManager.I != null &&
            NotePoolManager.I.RemainingCount > 0;

        if (notePossible)
            results.Add(new Result(ResultType.Note));

        // 3) 쓰레기(항상 존재)
        results.Add(new Result(ResultType.Trash));

        // 무작위 선택
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

        RefreshInteractable();
    }

    private void GiveItemOnce(string itemId)
    {
        Debug.Log($"[SEARCH] {objectId}: 아이템 획득 {itemId}");

        if (GameManager.I != null)
            GameManager.I.AddItem(itemId);

        _remainingOneTimeItems?.Remove(itemId);
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

    private void RefreshInteractable()
    {
        // “쓰레기만 남았으면” 잠금 + 호버/클릭 차단
        bool hasLoot = HasNonTrashRemaining();
        if (!hasLoot)
        {
            if (button != null) button.interactable = false;

            if (blockRaycastsWhenEmpty)
            {
                if (_cg == null) _cg = gameObject.AddComponent<CanvasGroup>();
                _cg.blocksRaycasts = false;  // ✅ 호버/클릭 자체 차단
                _cg.interactable = false;
            }
        }
        // hasLoot == true 일 때는 여기서 굳이 풀어주지 않음
        // (조건 해금은 RequireItemsAndFlagsToInteract가 담당할 수 있으니 충돌 방지)
    }

    public void AddInitialOneTimeItem(string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return;

        if (initialOneTimeItems == null) initialOneTimeItems = new List<string>();
        if (!initialOneTimeItems.Contains(itemId))
            initialOneTimeItems.Add(itemId);

        // 런타임 리스트도 같이 반영(맵 다시 들어올 때 말고 즉시 반영)
        if (_remainingOneTimeItems == null)
            _remainingOneTimeItems = new List<string>(initialOneTimeItems);
        else if (!_remainingOneTimeItems.Contains(itemId))
            _remainingOneTimeItems.Add(itemId);

        RefreshInteractable();
    }

    public void RemoveInitialOneTimeItem(string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return;

        if (initialOneTimeItems != null)
            initialOneTimeItems.Remove(itemId);

        if (_remainingOneTimeItems != null)
            _remainingOneTimeItems.Remove(itemId);

        RefreshInteractable();
    }
}
