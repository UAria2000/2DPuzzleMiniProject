using System.Collections.Generic;
using UnityEngine;

public class RandomUniqueItemAssigner : MonoBehaviour
{
    [Header("차키가 들어갈 후보(서랍 3개)")]
    public List<SearchableObject> targets = new();

    [Header("배치할 유니크 아이템 ID")]
    public string itemId = "car_key";

    [Header("혹시 이미 들어있을 수 있으니 시작 시 제거 후 배치")]
    public bool clearExisting = true;

    private void Awake()
    {
        if (targets == null || targets.Count == 0) return;

        if (clearExisting)
        {
            foreach (var t in targets)
                if (t != null) t.RemoveInitialOneTimeItem(itemId);
        }

        int idx = Random.Range(0, targets.Count);
        targets[idx].AddInitialOneTimeItem(itemId);

        Debug.Log($"[ASSIGN] {itemId} -> {targets[idx].name}");
    }
}
