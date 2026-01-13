using UnityEngine;

public class MetaResetButton : MonoBehaviour
{
    public void ResetMeta()
    {
        if (MetaSave.I == null)
        {
            Debug.LogError("[MetaResetButton] MetaSave.I is null (씬에 MetaSave가 없거나 Awake가 안 돌았음)");
            return;
        }

        MetaSave.I.ResetAllMeta();
        Debug.Log("[MetaResetButton] ResetMeta called");
    }
}
