using UnityEngine;
using UnityEngine.SceneManagement;

public class InventoryToggle : MonoBehaviour
{
    [Header("인벤토리 UI 패널(켜고/끄는 대상) - 비워도 됨(자동 탐색)")]
    public GameObject inventoryPanel;

    [Header("자동으로 찾을 패널 이름(Hierarchy 이름과 정확히 동일)")]
    public string inventoryPanelName = "InventoryPanel";

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        EnsurePanelReference();
        ForceClose(); // 시작은 닫힘
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 재시작(씬 리로드) 후 패널 참조가 끊길 수 있으니 다시 잡기
        inventoryPanel = null;
        EnsurePanelReference();
        ForceClose();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftAlt) ||
            Input.GetKeyDown(KeyCode.I) ||
            Input.GetKeyDown(KeyCode.E))
        {
            Toggle();
        }
    }

    private void EnsurePanelReference()
    {
        if (inventoryPanel != null) return;

        // 비활성 오브젝트도 찾기 위해 Transform 전체에서 이름 검색
        var all = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var t in all)
        {
            if (t.name == inventoryPanelName)
            {
                inventoryPanel = t.gameObject;
                break;
            }
        }
    }

    public void Toggle()
    {
        EnsurePanelReference();
        if (inventoryPanel == null)
        {
            Debug.LogWarning($"[InventoryToggle] inventoryPanel 못 찾음. 이름='{inventoryPanelName}' 확인!");
            return;
        }
        inventoryPanel.SetActive(!inventoryPanel.activeSelf);
    }

    public void ForceClose()
    {
        EnsurePanelReference();
        if (inventoryPanel == null) return;
        inventoryPanel.SetActive(false);
    }
}
