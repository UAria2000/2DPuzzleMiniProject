using System.Collections.Generic;
using UnityEngine;

public class LocationManager : MonoBehaviour
{
    public bool startWithNothing = true;
    public string startLocationId = "Basement";
    public static LocationManager I;

    [System.Serializable]
    public class LocationEntry
    {
        public string id;
        public GameObject panel;
    }

    public List<LocationEntry> locations = new();
    private Dictionary<string, GameObject> _map;

    private void Awake()
    {
        I = this;

        _map = new Dictionary<string, GameObject>();
        foreach (var e in locations)
        {
            if (e == null) continue;
            if (string.IsNullOrEmpty(e.id)) continue;
            if (e.panel == null) continue;

            _map[e.id] = e.panel;
        }
    }

    private void Start()
    {
        if (startWithNothing) HideAll();
        else Show(startLocationId);
    }

    public void Show(string id)
    {
        foreach (var kv in _map)
        {
            if (kv.Value != null)
                kv.Value.SetActive(kv.Key == id);
        }
    }

    //  추가: 타이틀 상태에서 맵 전부 끄기용
    public void HideAll()
    {
        foreach (var kv in _map)
        {
            if (kv.Value != null)
                kv.Value.SetActive(false);
        }
    }
}
