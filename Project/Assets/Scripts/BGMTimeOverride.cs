using UnityEngine;

public class BGMTimeOverride : MonoBehaviour
{
    [Header("몇 분부터 오버라이드? (4시간 = 240분)")]
    public int thresholdMinutes = 240;

    [Header("4시간 이후에 재생할 BGM")]
    public AudioClip overrideBgm;

    [Header("페이드 시간")]
    public float fadeTime = 0.7f;

    private bool _triggered;

    private void Update()
    {
        if (TimeManager.I == null || BGMPlayer.I == null) return;

        int m = TimeManager.I.minutes;

        // 4시간 이후: 오버라이드 시작
        if (!_triggered && m >= thresholdMinutes)
        {
            _triggered = true;
            if (overrideBgm != null)
                BGMPlayer.I.PlayOverride(overrideBgm, fadeTime);
        }

        // 재시작 등으로 시간이 다시 240 아래로 내려갔으면 복구
        if (_triggered && m < thresholdMinutes)
        {
            _triggered = false;
            BGMPlayer.I.ClearOverride(fadeTime);
        }
    }
}
