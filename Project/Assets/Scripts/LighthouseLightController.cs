using UnityEngine;
using UnityEngine.UI;

public class LighthouseLightController : MonoBehaviour
{
    [Header("ID (문자열 정확히 일치해야 함)")]
    public string bulbItemId = "등대용 전구";
    public string repairFlagId = "RepairLight";

    [Header("UI Button 오브젝트들")]
    public Button brokenLightButton;     // 부서진 등명기(버튼)
    public Button repairedLightButton;   // 수리된 등명기(버튼) - 있으면 연결, 없으면 비워도 됨

    [Header("등대 패널 배경 이미지 교체")]
    public Image lighthousePanelImage;   // 등대 배경 Image
    public Sprite intactLightSprite;     // Intact light 스프라이트

    private bool _listenerBound;

    private void Start()
    {
        // 수리된 등명기는 기본적으로 꺼두는 게 일반적
        // (RepairLight 켜지면 켜질 거임)
        SyncVisual();

        // 클릭 이벤트 연결(Inspector에서 OnClick으로 연결해도 되지만 자동이 편함)
        if (brokenLightButton != null && !_listenerBound)
        {
            brokenLightButton.onClick.AddListener(OnClickBrokenLight);
            _listenerBound = true;
        }
    }

    private void OnDestroy()
    {
        if (brokenLightButton != null && _listenerBound)
            brokenLightButton.onClick.RemoveListener(OnClickBrokenLight);
    }

    private void Update()
    {
        // 플래그/인벤 변화에 따라 버튼 상태 갱신(간단/안전)
        SyncVisual();
    }

    private void SyncVisual()
    {
        bool repaired = (FlagStore.I != null && FlagStore.I.Has(repairFlagId));
        bool hasBulb = (GameManager.I != null && GameManager.I.HasItem(bulbItemId));

        // 1) 부서진 등명기: 수리 전에는 보이고, 전구 있을 때만 클릭 가능
        if (brokenLightButton != null)
        {
            brokenLightButton.gameObject.SetActive(!repaired);
            brokenLightButton.interactable = (!repaired && hasBulb);
        }

        // 2) 수리된 등명기: 수리 후에만 보이게(원하면)
        if (repairedLightButton != null)
        {
            repairedLightButton.gameObject.SetActive(repaired);
        }

        // 3) 배경 이미지 교체(수리 후)
        if (repaired && lighthousePanelImage != null && intactLightSprite != null)
        {
            lighthousePanelImage.sprite = intactLightSprite;
        }
    }

    private void OnClickBrokenLight()
    {
        // 안전 체크
        if (GameManager.I == null || FlagStore.I == null) return;

        if (!GameManager.I.HasItem(bulbItemId))
        {
            Debug.Log("[Lighthouse] 전구 없음 → 수리 불가");
            return;
        }

        // RepairLight 플래그 ON
        FlagStore.I.Set(repairFlagId);
        Debug.Log("[Lighthouse] RepairLight SET");

        // 즉시 반영
        SyncVisual();
    }
}
