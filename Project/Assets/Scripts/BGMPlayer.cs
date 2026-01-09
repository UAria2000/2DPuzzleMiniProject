using System.Collections;
using UnityEngine;

public class BGMPlayer : MonoBehaviour
{
    public static BGMPlayer I;

    public AudioSource source;
    public float defaultFadeTime = 0.5f;
    public float maxVolume = 1f;

    private Coroutine _co;

    private AudioClip _current;
    private AudioClip _lastMapClip;

    public bool OverrideActive { get; private set; }
    public AudioClip OverrideClip { get; private set; }

    private void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);

        if (source == null)
        {
            source = GetComponent<AudioSource>();
            if (source == null) source = gameObject.AddComponent<AudioSource>();
        }

        source.loop = true;
        source.playOnAwake = false;
        source.volume = Mathf.Clamp01(maxVolume);
    }

    //  맵에서 호출하는 BGM (4시간 이후면 무시)
    public void PlayMap(AudioClip clip, float fadeTime = -1f)
    {
        if (clip == null) return;

        _lastMapClip = clip;

        if (OverrideActive) return; // 오버라이드 중엔 맵 BGM 무시
        PlayInternal(clip, fadeTime);
    }

    //  시간 조건으로 강제 재생하는 BGM
    public void PlayOverride(AudioClip clip, float fadeTime = -1f)
    {
        if (clip == null) return;

        OverrideActive = true;
        OverrideClip = clip;
        PlayInternal(clip, fadeTime);
    }

    //  재시작 등으로 오버라이드 해제(맵BGM로 복귀)
    public void ClearOverride(float fadeTime = -1f)
    {
        OverrideActive = false;
        OverrideClip = null;

        if (_lastMapClip != null)
            PlayInternal(_lastMapClip, fadeTime);
    }

    private void PlayInternal(AudioClip clip, float fadeTime)
    {
        if (_current == clip && source.isPlaying) return;

        if (fadeTime < 0f) fadeTime = defaultFadeTime;

        if (_co != null) StopCoroutine(_co);
        _co = StartCoroutine(CoFadeTo(clip, fadeTime));
    }

    private IEnumerator CoFadeTo(AudioClip next, float fade)
    {
        float startVol = source.volume;

        if (source.isPlaying && fade > 0f)
        {
            for (float t = 0; t < fade; t += Time.unscaledDeltaTime)
            {
                source.volume = Mathf.Lerp(startVol, 0f, t / fade);
                yield return null;
            }
        }

        source.volume = 0f;
        _current = next;
        source.clip = next;
        source.Play();

        float target = Mathf.Clamp01(maxVolume);
        if (fade > 0f)
        {
            for (float t = 0; t < fade; t += Time.unscaledDeltaTime)
            {
                source.volume = Mathf.Lerp(0f, target, t / fade);
                yield return null;
            }
        }

        source.volume = target;
        _co = null;
    }
}
