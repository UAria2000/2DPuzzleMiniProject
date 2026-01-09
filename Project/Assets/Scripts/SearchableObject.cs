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

    // 런타임 상태
    private List<string> _remainingOneTimeItems;
    private bool _noteAlreadyDroppedHere;

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
        // ResetRunState();  //  여기서 하지 말기
    }

    private void Start()
    {
        ResetRunState();     //  Start에서 초기화
    }

    public void AddInitialOneTimeItem(string itemId)
    {
        if (!initialOneTimeItems.Contains(itemId))
            initialOneTimeItems.Add(itemId);
    }

    public void RemoveInitialOneTimeItem(string itemId)
    {
        initialOneTimeItems.Remove(itemId);
    }

    // 새 회차 시작 시 호출하면 깔끔 (원하면 나중에 연결)
    public void ResetRunState()
    {
        _remainingOneTimeItems = new List<string>(initialOneTimeItems);
        _noteAlreadyDroppedHere = false;

        if (button != null) button.interactable = true;
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
        foreach (var id in _remainingOneTimeItems)
            results.Add(new Result(ResultType.Item, id));

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

        // 무작위 선택 (쓰레기 확률 = 1 / (가능결과 수) 형태)
        int pick = Random.Range(0, results.Count);
        var chosen = results[pick];

        switch (chosen.type)
        {
            case ResultType.Trash:
                // 허탕: 인벤에 안 넣고 메시지만
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

        // 인벤에 추가
        if (GameManager.I != null)
            GameManager.I.AddItem(itemId);

        // 1회성이므로 남은 목록에서 제거
        _remainingOneTimeItems.Remove(itemId);
    }

    private void GiveNote()
    {
        if (NotePoolManager.I == null) return;

        if (NotePoolManager.I.TryTakeRandomNote(out var noteId))
        {
            Debug.Log($"[SEARCH] {objectId}: 쪽지 획득 {noteId}");

            _noteAlreadyDroppedHere = true; // 이 오브젝트에서는 이후 쪽지 금지 :contentReference[oaicite:9]{index=9}

            if (GameManager.I != null)
                GameManager.I.AddItem(noteId);
        }
    }

    private void RefreshInteractable()
    {
        bool hasItems = _remainingOneTimeItems.Count > 0;

        bool notePossible =
            canDropNotes &&
            !_noteAlreadyDroppedHere &&
            NotePoolManager.I != null &&
            NotePoolManager.I.RemainingCount > 0;

        // 남은 게 쓰레기뿐이면 비활성화(false) :contentReference[oaicite:10]{index=10} :contentReference[oaicite:11]{index=11}
        if (!hasItems && !notePossible)
        {
            if (button != null)
                button.interactable = false;

            // “아예 안 보이게” 하고 싶으면 아래 줄도 켜기
            // gameObject.SetActive(false);
        }
    }
}
