using UnityEngine;
using UnityEngine.SceneManagement;

public class RunRestartManager : MonoBehaviour
{
    public static RunRestartManager I;

    [Header("시작 위치(LocationManager id)")]
    public string startLocationId = "Basement";

    private bool _restarting;

    private void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
    }

    public void RestartRun()
    {
        if (_restarting) return;
        _restarting = true;

        Debug.Log("[RESTART] RestartRun called");

        Time.timeScale = 1f;

        // 인벤 닫기(열려있는 상태로 리스타트 방지)
        foreach (var t in Object.FindObjectsByType<InventoryToggle>(
                     FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            t.ForceClose();
        }

        // 이번 회차(run)만 초기화
        if (GameManager.I != null) GameManager.I.ClearInventory();
        if (FlagStore.I != null) FlagStore.I.ResetAll();
        if (TimeManager.I != null) TimeManager.I.ResetTime();
        if (NotePoolManager.I != null) NotePoolManager.I.ResetPool();
        if (EndingManager.I != null) EndingManager.I.ResetRunState();

        SceneManager.sceneLoaded -= OnSceneLoaded_AfterRestart;
        SceneManager.sceneLoaded += OnSceneLoaded_AfterRestart;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnSceneLoaded_AfterRestart(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded_AfterRestart;

        // 탐색 오브젝트 런 상태 초기화
        foreach (var s in Object.FindObjectsByType<SearchableObject>(
                     FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            s.ResetRunState();
        }

        // 시작 위치 강제
        if (LocationManager.I != null)
            LocationManager.I.Show(startLocationId);

        // 엔딩 UI 숨김(있으면)
        if (UIEnding.I != null)
            UIEnding.I.Hide();

        // 인벤 닫기(안전)
        foreach (var t in Object.FindObjectsByType<InventoryToggle>(
                     FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            t.ForceClose();
        }

        _restarting = false;
        Debug.Log("[RESTART] Done");
    }
}
