using System;
using System.Collections.Generic;
using UnityEngine;

public class FlagStore : MonoBehaviour
{
    public static FlagStore I;

    public event Action<string> OnFlagSet;   //  추가: 플래그가 켜질 때 알림

    private HashSet<string> _flags = new();

    private void Awake()
    {
        //  중복 방지 + 유지
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool Has(string id) => _flags.Contains(id);

    public void Set(string id)
    {
        // 이미 켜진 플래그면 아무것도 안 함(중복 호출 방지)
        if (!_flags.Add(id)) return;

        Debug.Log($"[FLAG] SET {id}");
        OnFlagSet?.Invoke(id);   //  여기서 EndingManager가 즉시 반응
    }

    public void Clear(string id)
    {
        if (_flags.Remove(id))
            Debug.Log($"[FLAG] CLEAR {id}");
    }

    public void ResetAll()
    {
        _flags.Clear();
        Debug.Log("[FLAG] RESET ALL");
    }
}
