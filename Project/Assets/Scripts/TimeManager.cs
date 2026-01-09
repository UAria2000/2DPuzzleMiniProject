using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager I;

    public int minutes;
    public bool killerTriggered;

    private void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }
        I = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Spend(int addMinutes)
    {
        minutes += addMinutes;

        // UI는 재시작/씬로드 타이밍에 없을 수 있으니 안전하게
        if (UITime.I != null)
            UITime.I.Refresh(minutes);

        if (!killerTriggered && minutes >= 360)
        {
            killerTriggered = true;
            if (EndingManager.I != null)
                EndingManager.I.TriggerKillerEnding();
        }
    }

    public void ResetTime()
    {
        minutes = 0;
        killerTriggered = false;

        if (UITime.I != null)
            UITime.I.Refresh(minutes);
    }
}
