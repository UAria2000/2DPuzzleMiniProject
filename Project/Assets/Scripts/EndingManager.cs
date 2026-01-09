using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndingManager : MonoBehaviour
{
    public static EndingManager I;

    [Header("시간 초과 플래그")]
    public string timeOverFlagId = "timeOver";

    [Header("인벤토리에서 검사할 아이템 ID (총 우선)")]
    public string gunItemId = "총";
    public string knifeItemId = "칼";

    [Header("timeOver 시 켤 '포기 엔딩' 플래그 3종")]
    public string giveUpGunEndingFlagId = "GiveUp_Gun";
    public string giveUpKnifeEndingFlagId = "GiveUp_Knife";
    public string giveUpNoneEndingFlagId = "GiveUp_None";

    [Header("엔딩으로 취급할 플래그 목록")]
    public List<string> endingFlagIds = new();

    private HashSet<string> _endingSet = new();

    private bool _timeOverProcessed;
    private bool _endingTriggered;

    private void Awake()
    {
        //  중복 방지 + 유지
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);

        RebuildEndingSet();
    }

    private void OnEnable()
    {
        StartCoroutine(BindFlagStore());
    }

    private IEnumerator BindFlagStore()
    {
        while (FlagStore.I == null) yield return null;

        // 중복 구독 방지
        FlagStore.I.OnFlagSet -= HandleFlagSet;
        FlagStore.I.OnFlagSet += HandleFlagSet;
    }

    private void OnDisable()
    {
        if (FlagStore.I != null)
            FlagStore.I.OnFlagSet -= HandleFlagSet;
    }

    private void RebuildEndingSet()
    {
        _endingSet.Clear();
        foreach (var id in endingFlagIds)
            if (!string.IsNullOrEmpty(id))
                _endingSet.Add(id);
    }

    private void HandleFlagSet(string id)
    {
        if (_endingTriggered) return;

        // 1) timeOver면 포기 엔딩 분기 플래그를 켜준다(총>칼>없음)
        if (id == timeOverFlagId)
        {
            ProcessTimeOver();
            return;
        }

        // 2) 엔딩 플래그면 즉시 엔딩 처리
        if (_endingSet.Contains(id))
        {
            TriggerEnding(id);
        }
    }

    private void ProcessTimeOver()
    {
        if (_timeOverProcessed) return;
        if (!FlagStore.I.Has(timeOverFlagId)) return;

        _timeOverProcessed = true;

        string chosen = giveUpNoneEndingFlagId;
        if (GameManager.I != null)
        {
            if (GameManager.I.HasItem(gunItemId)) chosen = giveUpGunEndingFlagId;
            else if (GameManager.I.HasItem(knifeItemId)) chosen = giveUpKnifeEndingFlagId;
        }

        // chosen 플래그를 Set하면 HandleFlagSet이 다시 호출되어 엔딩 처리됨
        FlagStore.I.Set(chosen);
    }

    private void TriggerEnding(string endingFlagId)
    {
        _endingTriggered = true;

        if (MetaSave.I != null)
            MetaSave.I.MarkSeen(endingFlagId);

        //  컷신이 있으면 먼저 재생하고, 끝나면 엔딩 UI 띄우기
        if (EndingCutscenePlayer.I != null && EndingCutscenePlayer.I.HasCutscene(endingFlagId))
        {
            EndingCutscenePlayer.I.Play(endingFlagId, () =>
            {
                if (UIEnding.I != null) UIEnding.I.Show(endingFlagId);
                Debug.Log($"[ENDING] {endingFlagId}");
            });
            return;
        }

        // 컷신 없으면 바로 엔딩 UI
        if (UIEnding.I != null) UIEnding.I.Show(endingFlagId);
        Debug.Log($"[ENDING] {endingFlagId}");
    }

    public void ResetRunState()
    {
        _timeOverProcessed = false;
        _endingTriggered = false;
        // endingFlagIds는 인스펙터 값이므로 유지
        RebuildEndingSet();
    }

    // 버튼/오브젝트에서 바로 엔딩 띄우고 싶을 때 사용 가능
    public void Trigger(string endingFlagId)
    {
        if (FlagStore.I != null) FlagStore.I.Set(endingFlagId);
        // Set 이벤트로 TriggerEnding이 자동 호출됨
    }

    public void TriggerKillerEnding()
    {
        string chosen = giveUpNoneEndingFlagId;
        if (GameManager.I != null)
        {
            if (GameManager.I.HasItem(gunItemId)) chosen = giveUpGunEndingFlagId;
            else if (GameManager.I.HasItem(knifeItemId)) chosen = giveUpKnifeEndingFlagId;
        }
        Trigger(chosen);
    }
}


