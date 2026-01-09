using UnityEngine;

public class UIRestartButton : MonoBehaviour
{
    public void Restart()
    {
        if (RunRestartManager.I != null)
            RunRestartManager.I.RestartRun();
        else
            Debug.LogWarning("[UIRestartButton] RunRestartManager.I is null");
    }
}
