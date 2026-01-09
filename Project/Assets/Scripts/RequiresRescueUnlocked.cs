using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class RequiresRescueUnlocked : MonoBehaviour
{
    private Button _btn;

    private void Awake()
    {
        _btn = GetComponent<Button>();
    }

    private void OnEnable()
    {
        Refresh();
        InvokeRepeating(nameof(Refresh), 0f, 0.5f); // 초보자용 간단 갱신
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(Refresh));
    }

    private void Refresh()
    {
        _btn.interactable = (MetaSave.I != null && MetaSave.I.RescueUnlocked);
    }
}
