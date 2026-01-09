using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class VolumeSettingsUI : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private Slider volumeSlider;     // VolumeSlider
    [SerializeField] private Image volumeIcon;        // VolumeIcon

    [Header("아이콘 4개 (0%, 1~33%, 34~66%, 67~100%)")]
    [SerializeField] private Sprite iconMute;
    [SerializeField] private Sprite iconLow;
    [SerializeField] private Sprite iconMid;
    [SerializeField] private Sprite iconHigh;

    [Header("AudioMixer 사용(선택)")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string mixerParamName = "MasterVolume"; // 노출 파라미터 이름

    private const string PrefKey = "masterVolume01"; // 0~1 저장

    private void Awake()
    {
        if (volumeSlider != null)
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
    }

    private void OnDestroy()
    {
        if (volumeSlider != null)
            volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
    }

    private void OnEnable()
    {
        // 옵션 패널 열릴 때 저장된 값으로 동기화
        float v = PlayerPrefs.GetFloat(PrefKey, 1f);
        if (volumeSlider != null)
            volumeSlider.SetValueWithoutNotify(v);

        ApplyVolume(v);
        UpdateIcon(v);
    }

    private void OnVolumeChanged(float v)
    {
        ApplyVolume(v);
        UpdateIcon(v);

        PlayerPrefs.SetFloat(PrefKey, v);
        PlayerPrefs.Save();
    }

    private void ApplyVolume(float v01)
    {
        // 1) AudioMixer가 연결돼 있으면 그걸로 제어(추천)
        if (audioMixer != null)
        {
            // 0이면 -80dB(거의 무음), 그 외는 로그 스케일
            float db = (v01 <= 0.0001f) ? -80f : Mathf.Log10(v01) * 20f;
            audioMixer.SetFloat(mixerParamName, db);
            return;
        }

        // 2) 아니면 간단하게 전체 볼륨
        AudioListener.volume = v01;
    }

    private void UpdateIcon(float v01)
    {
        if (volumeIcon == null) return;

        if (v01 <= 0.0001f) volumeIcon.sprite = iconMute;
        else if (v01 <= 0.33f) volumeIcon.sprite = iconLow;
        else if (v01 <= 0.66f) volumeIcon.sprite = iconMid;
        else volumeIcon.sprite = iconHigh;
    }
}
