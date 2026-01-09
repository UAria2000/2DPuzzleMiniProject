using UnityEngine;

public class StartMenu : MonoBehaviour
{
    [Header("시작 메뉴 패널(시작 누르면 꺼질 대상)")]
    public GameObject startMenuPanel;

    [Header("옵션 패널(O로 토글)")]
    public GameObject optionPanel;

    private void Awake()
    {
        // 옵션은 시작 시 닫힘
        if (optionPanel != null) optionPanel.SetActive(false);
    }

    private void Update()
    {
        // 시작 메뉴가 꺼진 뒤에도 옵션 토글 가능
        if (Input.GetKeyDown(KeyCode.O))
        {
            ToggleOption();
        }
    }

    // Start 버튼
    public void OnClickStart()
    {
        if (startMenuPanel != null)
            startMenuPanel.SetActive(false);

        if (LocationManager.I != null)
            LocationManager.I.Show("Basement");
    }

    // Option 버튼(시작 메뉴에 있는 옵션 버튼)
    public void OnClickOption()
    {
        if (optionPanel != null)
            optionPanel.SetActive(true);
    }

    // 옵션 패널 안 닫기 버튼
    public void OnClickCloseOption()
    {
        if (optionPanel != null)
            optionPanel.SetActive(false);
    }

    // Quit 버튼
    public void OnClickQuit()
    {
        Application.Quit();
        Debug.Log("[StartMenu] Quit");
    }

    public void ToggleOption()
    {
        if (optionPanel == null) return;
        optionPanel.SetActive(!optionPanel.activeSelf);
    }
}
