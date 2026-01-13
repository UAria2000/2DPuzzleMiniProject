using System.Collections.Generic;
using UnityEngine;

public class MetaSave : MonoBehaviour
{
    public static MetaSave I;

    // 본 엔딩 플래그들(메타)
    private HashSet<string> _seenEndings = new();

    // 저장 키
    private const string SeenKey = "meta_seen_endings_v2";

    // 구조루트 해금 여부
    public bool RescueUnlocked { get; private set; }

    private void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }

        I = this;
        DontDestroyOnLoad(gameObject);

        Load();
    }
    public bool HasSeen(string endingFlagId) => _seenEndings.Contains(endingFlagId);

    // 엔딩을 봤다고 기록
    public void MarkSeen(string endingFlagId)
    {
        if (string.IsNullOrEmpty(endingFlagId)) return;

        if (_seenEndings.Add(endingFlagId))
        {
            Save();
            RecalcRescueUnlocked();
        }
    }

    private void RecalcRescueUnlocked()
    {
        //  네 프로젝트 플래그 이름 기준
        bool land = HasAny("WalkEnding", "CarEnding", "BicycleEnding");
        bool sea = HasAny("SwimEnding", "RubberBoatEnding", "MotorBoatEnding");
        bool killer = HasAny("GiveUp_Gun", "GiveUp_Knife", "GiveUp_None");

        RescueUnlocked = land && sea && killer;
        PlayerPrefs.SetInt("rescueUnlocked", RescueUnlocked ? 1 : 0);
        PlayerPrefs.Save();
    }

    private bool HasAny(params string[] ids)
    {
        foreach (var id in ids)
            if (_seenEndings.Contains(id))
                return true;
        return false;
    }

    private void Save()
    {
        string s = string.Join("|", _seenEndings);
        PlayerPrefs.SetString(SeenKey, s);
        PlayerPrefs.Save();
    }

    private void Load()
    {
        _seenEndings.Clear();

        string s = PlayerPrefs.GetString(SeenKey, "");
        if (!string.IsNullOrEmpty(s))
        {
            foreach (var e in s.Split('|'))
                if (!string.IsNullOrEmpty(e))
                    _seenEndings.Add(e);
        }

        // RescueUnlocked는 SeenEndings로 재계산하는 게 맞아서 여기선 읽기만(디버그용)
        RescueUnlocked = PlayerPrefs.GetInt("rescueUnlocked", 0) == 1;
    }

    // (선택) 개발용: 메타 초기화
    public void ResetAllMeta()
    {
        _seenEndings.Clear();

        // 저장키를 확실히 삭제
        PlayerPrefs.DeleteKey(SeenKey);
        PlayerPrefs.DeleteKey("rescueUnlocked");
        PlayerPrefs.Save();

        // 메모리/상태도 확실히 초기화
        RescueUnlocked = false;

        Debug.Log("[MetaSave] ResetAllMeta: cleared seen endings + deleted PlayerPrefs keys");
    }
}

