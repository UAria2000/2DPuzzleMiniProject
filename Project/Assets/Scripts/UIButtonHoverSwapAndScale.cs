using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIButtonHoverSwapAndScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("이미지 바꿀 대상(비우면 자기 자신 Image 사용)")]
    public Image targetImage;

    [Header("호버 시 사용할 스프라이트(비우면 이미지 교체 안 함)")]
    public Sprite hoverSprite;

    [Header("기본 스프라이트(비우면 시작 시점 스프라이트를 자동 저장)")]
    public Sprite normalSprite;

    [Header("호버 시 배율")]
    public float hoverScale = 1.08f;

    [Header("부드러운 전환 속도(클수록 빠름)")]
    public float speed = 12f;

    private Vector3 _baseScale;
    private Vector3 _targetScale;
    private bool _hovering;

    private void Awake()
    {
        if (targetImage == null)
            targetImage = GetComponent<Image>();

        _baseScale = transform.localScale;
        _targetScale = _baseScale;

        if (targetImage != null && normalSprite == null)
            normalSprite = targetImage.sprite;
    }

    private void Update()
    {
        // timeScale이 0이어도(시작화면) 애니메이션 되게 unscaledDeltaTime 사용
        transform.localScale = Vector3.Lerp(transform.localScale, _targetScale, Time.unscaledDeltaTime * speed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _hovering = true;
        _targetScale = _baseScale * hoverScale;

        if (targetImage != null && hoverSprite != null)
            targetImage.sprite = hoverSprite;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _hovering = false;
        _targetScale = _baseScale;

        if (targetImage != null && normalSprite != null)
            targetImage.sprite = normalSprite;
    }

    private void OnDisable()
    {
        // 패널 꺼졌다 켜져도 원상복귀
        transform.localScale = _baseScale;
        _targetScale = _baseScale;

        if (targetImage != null && normalSprite != null)
            targetImage.sprite = normalSprite;

        _hovering = false;
    }
}
