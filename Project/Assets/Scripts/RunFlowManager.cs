using UnityEngine;
using UnityEngine.SceneManagement;

public class RunFlowManager : MonoBehaviour
{
    public static RunFlowManager I;

    [Header("UI 오브젝트 이름(씬에서 이 이름으로 찾음)")]
    public string startMenuPanelName = "StartMenuPanel";
    public string optionPanelName = "OptionPanel";
    public string introPanelName = "IntroPanel"; // 있으면 꺼줌(없으면 무시)

    private bool _busy;

    private void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
    }

    // 옵션의 "타이틀" 버튼이 호출
    public void GoToTitle()
    {
        if (_busy) return;
        _busy = true;

        Time.timeScale = 1f;

        // 인벤 닫기
        foreach (var t in Object.FindObjectsByType<InventoryToggle>(
            FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            t.ForceClose();
        }

        //  이번 회차(run)만 초기화 (메타는 유지)
        GameManager.I?.ClearInventory();
        FlagStore.I?.ResetAll();
        TimeManager.I?.ResetTime();
        NotePoolManager.I?.ResetPool();
        EndingManager.I?.ResetRunState();

        SceneManager.sceneLoaded -= OnSceneLoaded_Title;
        SceneManager.sceneLoaded += OnSceneLoaded_Title;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnSceneLoaded_Title(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded_Title;

        // 장소 전부 끄기(타이틀에서는 아무 맵도 안 보이게)
        LocationManager.I?.HideAll();

        // 인트로 패널이 있으면 끔(타이틀 복귀 시 인트로 다시 안 보이게)
        var intro = FindByName(introPanelName);
        if (intro != null) intro.SetActive(false);

        // 타이틀(StartMenuPanel) 켜기
        var menu = FindByName(startMenuPanelName);
        if (menu != null) menu.SetActive(true);

        // 옵션 패널 닫기
        var opt = FindByName(optionPanelName);
        if (opt != null) opt.SetActive(false);

        // 엔딩 UI 숨김(혹시 남아있을까 안전)
        if (UIEnding.I != null) UIEnding.I.Hide();

        // 인벤도 닫기(안전)
        foreach (var t in Object.FindObjectsByType<InventoryToggle>(
            FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            t.ForceClose();
        }

        _busy = false;
    }

    private GameObject FindByName(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;

        var all = Object.FindObjectsByType<Transform>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var t in all)
            if (t.name == name) return t.gameObject;

        return null;
    }
}
