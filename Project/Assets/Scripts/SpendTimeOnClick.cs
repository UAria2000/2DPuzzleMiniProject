using UnityEngine;

public class SpendTimeOnClick : MonoBehaviour
{
    public int minutesToSpend = 5;

    public void Spend()
    {
        if (TimeManager.I == null)
        {
            Debug.LogWarning("[TIME] TimeManager.I == null (시간 소모 실패)");
            return;
        }

        int before = TimeManager.I.minutes;
        TimeManager.I.Spend(minutesToSpend);
        Debug.Log($"[TIME] +{minutesToSpend} ({before} -> {TimeManager.I.minutes})");
    }
}
