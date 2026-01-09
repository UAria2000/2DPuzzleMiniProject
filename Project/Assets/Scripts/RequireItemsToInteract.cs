using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class RequireItemsToInteract : MonoBehaviour
{
    [Header("이 아이템들을 모두 가지고 있어야 클릭 가능")]
    public List<string> requiredItems = new();

    [Header("잠겨있을 때 버튼을 회색/투명 처리하고 싶으면 CanvasGroup 사용")]
    public bool visuallyHideWhenLocked = false;

    private Button _button;
    private CanvasGroup _group;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _group = GetComponent<CanvasGroup>(); // 있으면 사용
    }

    private void OnEnable()
    {
        StartCoroutine(InitWhenReady());
    }

    private IEnumerator InitWhenReady()
    {
        // GameManager 생성 순서 문제 방지
        while (GameManager.I == null) yield return null;

        GameManager.I.InventoryChanged += Evaluate;
        Evaluate();
    }

    private void OnDisable()
    {
        if (GameManager.I != null)
            GameManager.I.InventoryChanged -= Evaluate;
    }

    public void Evaluate()
    {
        bool ok = AreRequirementsMet();

        _button.interactable = ok;

        // "비활성화 상태" 시각 처리(선택)
        if (visuallyHideWhenLocked)
        {
            // SetActive(false)는 나중에 자동 재활성화가 어려우니 CanvasGroup 권장
            if (_group == null) _group = gameObject.AddComponent<CanvasGroup>();

            _group.alpha = ok ? 1f : 0f;          // 안 보이게
            _group.blocksRaycasts = ok;           // 클릭 막기
            _group.interactable = ok;
        }
    }

    private bool AreRequirementsMet()
    {
        if (GameManager.I == null) return false;

        foreach (var id in requiredItems)
            if (!GameManager.I.HasItem(id))
                return false;

        return true;
    }
}
