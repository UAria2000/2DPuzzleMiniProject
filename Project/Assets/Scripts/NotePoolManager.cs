using System.Collections.Generic;
using UnityEngine;

public class NotePoolManager : MonoBehaviour
{
    public static NotePoolManager I;

    public List<string> initialNotes = new()
    {
        "note_1","note_2","note_3","note_4",
        "note_H","note_U","note_N","note_T","note_E","note_R"
    };

    private List<string> _remaining;

    private void Awake()
    {
        if (I != null) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);

        ResetPool();
    }

    public void ResetPool()
    {
        _remaining = new List<string>(initialNotes);
        Debug.Log($"[NOTEPOOL] Reset. remaining={_remaining.Count}");
    }

    public int RemainingCount => _remaining?.Count ?? 0;

    public bool TryTakeRandomNote(out string noteId)
    {
        noteId = null;
        if (_remaining == null || _remaining.Count == 0) return false;

        int idx = Random.Range(0, _remaining.Count);
        noteId = _remaining[idx];
        _remaining.RemoveAt(idx);
        return true;
    }
}
