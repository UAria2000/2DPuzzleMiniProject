using TMPro;
using UnityEngine;

public class UITime : MonoBehaviour
{
    public static UITime I;
    public TextMeshProUGUI text;

    private void Awake()
    {
        I = this;

        // 인스펙터에 안 넣었으면 자동으로 찾기(자기 자신/자식 포함)
        if (text == null)
            text = GetComponent<TextMeshProUGUI>() ?? GetComponentInChildren<TextMeshProUGUI>(true);
    }

    private void OnEnable()
    {
        // 씬 재시작 직후 UI가 새로 생겨도 현재 시간으로 동기화
        if (TimeManager.I != null)
            Refresh(TimeManager.I.minutes);
    }

    public void Refresh(int minutes)
    {
        if (text == null) return;

        int h = minutes / 60;
        int m = minutes % 60;
        text.text = $"{h}:{m:00}";
    }
}
