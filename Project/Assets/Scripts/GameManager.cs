using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager I;

    private HashSet<string> _items = new();

    public event Action InventoryChanged;   //  추가

    private void Awake()
    {
        if (I != null) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool HasItem(string id) => _items.Contains(id);

    public void AddItem(string id)
    {
        _items.Add(id);

        // 인벤 UI 갱신(있으면)
        if (UIInventory.I != null)
            UIInventory.I.Refresh(_items);

        // 인벤 변경 알림
        InventoryChanged?.Invoke();
    }

    // (선택) 새 회차 시작 시 호출용
    public void ClearInventory()
    {
        _items.Clear();
        if (UIInventory.I != null) UIInventory.I.Refresh(_items);
        InventoryChanged?.Invoke();
    }
}
