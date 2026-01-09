using UnityEngine;
using UnityEngine.UI;

public class DisableButtonIfHasItem : MonoBehaviour
{
    [Header("검사할 아이템 ID (예: 총)")]
    public string itemId = "총";

    [Header("대상 버튼(비우면 자기 자신 버튼 사용)")]
    public Button targetButton;

    [Header("true면 버튼을 아예 숨김(SetActive(false))")]
    public bool hideGameObjectInstead = false;

    private void Awake()
    {
        if (targetButton == null)
            targetButton = GetComponent<Button>();
    }

    private void OnEnable()
    {
        Refresh();
    }

    private void Update()
    {
        // 초보자용: 인벤 변화를 이벤트로 안 받는 구조면 Update로 체크(가볍게)
        Refresh();
    }

    private void Refresh()
    {
        if (GameManager.I == null || string.IsNullOrEmpty(itemId)) return;

        bool has = GameManager.I.HasItem(itemId);

        if (hideGameObjectInstead)
        {
            // 총 있으면 숨김
            if (targetButton != null)
                targetButton.gameObject.SetActive(!has);
            else
                gameObject.SetActive(!has);
        }
        else
        {
            // 총 있으면 클릭 불가
            if (targetButton != null)
                targetButton.interactable = !has;
        }
    }
}
