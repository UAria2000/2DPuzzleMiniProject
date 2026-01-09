using UnityEngine;

public class SetFlagOnClick : MonoBehaviour
{
    public string flagId;

    public void SetFlag()
    {
        FlagStore.I.Set(flagId);
    }
}
