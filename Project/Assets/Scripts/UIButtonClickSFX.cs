using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIButtonClickSFX : MonoBehaviour
{
    public static UIButtonClickSFX I;

    [Header("클릭 효과음(하나만)")]
    public AudioClip clickClip;

    [Header("볼륨")]
    [Range(0f, 1f)] public float volume = 1f;

    [Header("중복 등록 방지용")]
    private readonly HashSet<int> _hooked = new();

    private AudioSource _source;

    private void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);

        _source = GetComponent<AudioSource>();
        if (_source == null) _source = gameObject.AddComponent<AudioSource>();

        _source.playOnAwake = false;
        _source.loop = false;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        HookAllButtonsInScene();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 새 씬 로드될 때마다 버튼 다시 훅
        HookAllButtonsInScene();
    }

    private void HookAllButtonsInScene()
    {
        // 비활성 포함해서 모두 찾기
        var buttons = Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var b in buttons)
        {
            if (b == null) continue;

            int id = b.GetInstanceID();
            if (_hooked.Contains(id)) continue;

            _hooked.Add(id);

            // 버튼 클릭 시 효과음 재생
            b.onClick.AddListener(PlayClick);
        }
    }

    public void PlayClick()
    {
        if (clickClip == null || _source == null) return;
        _source.PlayOneShot(clickClip, volume);
    }
}
