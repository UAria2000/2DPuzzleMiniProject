using UnityEngine;

public class GoToLocationButton : MonoBehaviour
{
    public string targetLocationId;

    public void Go()
    {
        LocationManager.I.Show(targetLocationId);
    }
}