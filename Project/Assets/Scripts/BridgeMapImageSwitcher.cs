using UnityEngine;
using UnityEngine.UI;

public class BridgeMapImageSwitcher : MonoBehaviour
{
    [Header("FlagStore에 저장된 도개교 가동 플래그 이름")]
    public string bridgeFlagId = "bridgeActivated";

    [Header("가동 전/후 이미지")]
    public Sprite beforeSprite;
    public Sprite afterSprite;

    [Header("비워두면 자기 Image를 자동 사용")]
    public Image target;

    private void Awake()
    {
        if (target == null) target = GetComponent<Image>();
    }

    // 다리 패널이 켜질 때마다 자동으로 최신 상태 반영
    private void OnEnable()
    {
        Refresh();
    }

    // 필요하면 버튼 OnClick에서 직접 호출 가능
    public void Refresh()
    {
        if (target == null || beforeSprite == null || afterSprite == null) return;

        bool activated = (FlagStore.I != null && FlagStore.I.Has(bridgeFlagId));
        target.sprite = activated ? afterSprite : beforeSprite;
    }
}
