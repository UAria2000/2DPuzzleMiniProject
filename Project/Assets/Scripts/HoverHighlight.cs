using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class HoverHighlight : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Optional: 비워두면 같은 오브젝트의 Outline을 찾음")]
    public Outline outline;

    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();

        if (outline == null)
            outline = GetComponent<Outline>();

        if (outline != null)
            outline.enabled = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (outline == null) return;

        // 클릭 가능한 상태일 때만 하이라이트
        if (_button.interactable)
            outline.enabled = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (outline == null) return;
        outline.enabled = false;
    }

    private void Update()
    {
        // hover 중에 버튼이 비활성화되면 하이라이트도 바로 끔
        if (outline != null && outline.enabled && !_button.interactable)
            outline.enabled = false;
    }

    private void OnDisable()
    {
        if (outline != null)
            outline.enabled = false;
    }
}
