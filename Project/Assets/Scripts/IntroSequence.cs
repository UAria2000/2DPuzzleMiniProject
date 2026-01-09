using UnityEngine;
using UnityEngine.Video;

public class IntroSequence : MonoBehaviour
{
    [Header("패널")]
    public GameObject introPanel;
    public GameObject startMenuPanel;

    [Header("비디오")]
    public VideoPlayer videoPlayer;

    [Header("스킵 키(원하면)")]
    public bool allowSkip = true;
    public KeyCode skipKey = KeyCode.Space;

    private bool _done;

    private void Awake()
    {
        // 인트로 동안엔 정상 시간
        Time.timeScale = 1f;

        if (introPanel != null) introPanel.SetActive(true);
        if (startMenuPanel != null) startMenuPanel.SetActive(false);
    }

    private void OnEnable()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoEnd;
            videoPlayer.errorReceived += OnVideoError;
        }
    }

    private void OnDisable()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoEnd;
            videoPlayer.errorReceived -= OnVideoError;
        }
    }

    private void Start()
    {
        if (videoPlayer == null)
        {
            Finish();
            return;
        }

        videoPlayer.Play();
    }

    private void Update()
    {
        if (_done) return;
        if (!allowSkip) return;

        if (Input.GetKeyDown(skipKey))
            Finish();
    }

    private void OnVideoEnd(VideoPlayer vp) => Finish();

    private void OnVideoError(VideoPlayer vp, string msg)
    {
        Debug.LogWarning("[IntroSequence] Video error: " + msg);
        Finish();
    }

    public void Finish()
    {
        if (_done) return;
        _done = true;

        if (videoPlayer != null) videoPlayer.Stop();

        if (introPanel != null) introPanel.SetActive(false);
        if (startMenuPanel != null) startMenuPanel.SetActive(true);
    }
}
