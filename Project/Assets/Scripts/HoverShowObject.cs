using UnityEngine;
using UnityEngine.EventSystems;

public class HoverShowObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject target; // B 이미지 오브젝트

    private void Start()
    {
        if (target != null) target.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (target != null) target.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (target != null) target.SetActive(false);
    }

    private void OnDisable()
    {
        if (target != null) target.SetActive(false);
    }
}
