using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class RequireFlagToInteract : MonoBehaviour
{
    public string requiredFlagId;  // 예: bridgeActivated

    private Button _btn;

    private void Awake()
    {
        _btn = GetComponent<Button>();
    }

    private void OnEnable()
    {
        Refresh();
    }

    private void Update()
    {
        // 초보자용: 매 프레임 갱신(가장 단순)
        Refresh();
    }

    private void Refresh()
    {
        if (FlagStore.I == null) return;
        _btn.interactable = FlagStore.I.Has(requiredFlagId);
    }
}
