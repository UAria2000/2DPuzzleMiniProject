using UnityEngine;

public class UIButtonGoTitle : MonoBehaviour
{
    public void GoTitle()
    {
        if (RunFlowManager.I != null) RunFlowManager.I.GoToTitle();
        else Debug.LogWarning("[UIButtonGoTitle] RunFlowManager가 씬에 없습니다.");
    }
}
