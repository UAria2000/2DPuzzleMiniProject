using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIEnding : MonoBehaviour
{
    public static UIEnding I;

    [System.Serializable]
    public class EndingText
    {
        public string endingFlagId;   // 예: SwimEnding
        public string title;          // 예: "수영 엔딩"
        [TextArea(2, 6)]
        public string body;           // 예: "구명조끼를 입고 바다로 뛰어들었다..."
    }

    [Header("UI 연결")]
    public GameObject panel;               // EndingPanel
    public TextMeshProUGUI titleText;      // TitleText
    public TextMeshProUGUI bodyText;       // BodyText

    [Header("엔딩 문구 목록")]
    public List<EndingText> endingTexts = new();

    private Dictionary<string, EndingText> _map;

    private void Awake()
    {
        I = this;

        // 빠른 조회용 딕셔너리 구성
        _map = new Dictionary<string, EndingText>();
        foreach (var e in endingTexts)
        {
            if (!string.IsNullOrEmpty(e.endingFlagId))
                _map[e.endingFlagId] = e;
        }

        if (panel != null)
            panel.SetActive(false);
    }

    public void Show(string endingFlagId)
    {
        if (panel != null) panel.SetActive(true);

        // 기본값(매핑 없을 때)
        string t = endingFlagId;
        string b = "";

        if (_map != null && _map.TryGetValue(endingFlagId, out var data))
        {
            if (!string.IsNullOrEmpty(data.title)) t = data.title;
            b = data.body;
        }

        if (titleText != null) titleText.text = t;
        if (bodyText != null) bodyText.text = b;

        // (선택) 엔딩 동안 게임 정지
        Time.timeScale = 0f;
    }

    public void Hide()
    {
        Time.timeScale = 1f;
        if (panel != null) panel.SetActive(false);
    }

    // Restart 버튼에서 호출할 함수
    public void RestartRun()
    {

        Time.timeScale = 1f;

        foreach (var t in Object.FindObjectsByType<InventoryToggle>(
             FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            t.ForceClose();
        }

        // 런(이번 회차)만 리셋, 메타(MetaSave)는 건드리지 않음!
        if (GameManager.I != null) GameManager.I.ClearInventory();
        if (FlagStore.I != null) FlagStore.I.ResetAll();
        if (TimeManager.I != null) TimeManager.I.ResetTime();
        if (NotePoolManager.I != null) NotePoolManager.I.ResetPool();
        if (EndingManager.I != null) EndingManager.I.ResetRunState();

        // 씬 로드 후 Basement로 강제 이동(중복 등록 방지)
        SceneManager.sceneLoaded -= OnSceneLoaded_ResetToBasement;
        SceneManager.sceneLoaded += OnSceneLoaded_ResetToBasement;

        var scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.buildIndex);
    }

    private void OnSceneLoaded_ResetToBasement(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded_ResetToBasement;

        // 1) 쪽지 풀 리셋(로드 후에 해야 안전)
        if (NotePoolManager.I != null)
            NotePoolManager.I.ResetPool();

        // 2) 모든 탐색 오브젝트 런 상태 리셋(쪽지 드랍 가능 상태로)
        foreach (var s in Object.FindObjectsByType<SearchableObject>(
                     FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            s.ResetRunState();
        }

        // 3) 시작 장소 강제
        if (LocationManager.I != null)
            LocationManager.I.Show("Basement");

        if (TimeManager.I != null)
            TimeManager.I.ResetTime();

        foreach (var t in Object.FindObjectsByType<InventoryToggle>(
             FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            t.ForceClose();
        }

        if (panel != null) panel.SetActive(false);
    }
}

