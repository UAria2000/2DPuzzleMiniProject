using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIInventory : MonoBehaviour
{
    public static UIInventory I;

    public Transform content;
    public TextMeshProUGUI itemTextPrefab;

    private void Awake() => I = this;

    public void Refresh(IEnumerable<string> items)
    {
        foreach (Transform c in content) Destroy(c.gameObject);

        foreach (var id in items)
        {
            var t = Instantiate(itemTextPrefab, content);
            t.text = id;
        }
    }
}
