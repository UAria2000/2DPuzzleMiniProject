using System.Collections;
using UnityEngine;

public class TimeLimitFlagTrigger : MonoBehaviour
{
    [Header("제한 시간(분). 예: 6시간=360")]
    public int limitMinutes = 360;

    [Header("시간 초과 시 켜질 플래그 ID")]
    public string flagId = "timeOver";

    [Header("한 번만 발동")]
    public bool triggerOnce = true;

    private bool _triggered;

    private void OnEnable()
    {
        StartCoroutine(WaitThenInit());
    }

    private IEnumerator WaitThenInit()
    {
        // Managers 초기화 순서 문제 방지
        while (TimeManager.I == null || FlagStore.I == null)
            yield return null;

        _triggered = false;
    }

    private void Update()
    {
        if (TimeManager.I == null || FlagStore.I == null) return;

        if (triggerOnce && _triggered) return;

        if (TimeManager.I.minutes >= limitMinutes)
        {
            // 이미 플래그가 켜져있으면 중복 Set 방지
            if (!FlagStore.I.Has(flagId))
                FlagStore.I.Set(flagId);

            _triggered = true;
        }
    }
}
