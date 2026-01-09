using System;
using UnityEngine;
using UnityEngine.Video;

public class EndingCutscenePlayer : MonoBehaviour
{
    public static EndingCutscenePlayer I;

    [Header("컷신 대상 엔딩 ID")]
    public string targetEndingId = "GoBackHomeSafty";

    [Header("재생할 비디오 클립")]
    public VideoClip clip;

    [Header("컷신 UI 패널(켜고/끄는 용도)")]
    public GameObject panel;

    [Header("VideoPlayer")]
    public VideoPlayer player;

    [Header("스킵")]
    public bool allowSkip = true;
    public KeyCode skipKey = KeyCode.Space;

    private Action _onFinished;
    private bool _playing;

    private void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;

        // 씬 재시작해도 안정적으로 쓰고 싶으면 켜기(선택)
        // DontDestroyOnLoad(gameObject);

        if (panel != null) panel.SetActive(false);
    }

    private void OnEnable()
    {
        if (player != null)
            player.loopPointReached += OnVideoEnd;
    }

    private void OnDisable()
    {
        if (player != null)
            player.loopPointReached -= OnVideoEnd;
    }

    private void Update()
    {
        if (!_playing) return;
        if (!allowSkip) return;

        if (Input.GetKeyDown(skipKey))
            Finish();
    }

    public bool HasCutscene(string endingId)
    {
        return endingId == targetEndingId && clip != null && player != null && panel != null;
    }

    public void Play(string endingId, Action onFinished)
    {
        if (!HasCutscene(endingId))
        {
            onFinished?.Invoke();
            return;
        }

        _onFinished = onFinished;
        _playing = true;

        // 컷신은 정상 시간으로 재생
        Time.timeScale = 1f;

        panel.SetActive(true);

        player.clip = clip;
        player.Stop();
        player.Play();
    }

    private void OnVideoEnd(VideoPlayer vp) => Finish();

    private void Finish()
    {
        if (!_playing) return;
        _playing = false;

        if (player != null) player.Stop();
        if (panel != null) panel.SetActive(false);

        var cb = _onFinished;
        _onFinished = null;
        cb?.Invoke();
    }
}
