using UnityEngine;

public class MapBGMTrigger : MonoBehaviour
{
    public AudioClip bgm;
    public float fadeTime = 0.5f;

    private void OnEnable()
    {
        if (BGMPlayer.I != null && bgm != null)
            BGMPlayer.I.PlayMap(bgm, fadeTime);
    }
}