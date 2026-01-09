using UnityEngine;

public class PickupButton : MonoBehaviour
{
    public string itemId;
    public GameObject objectToHide; // 안 보이게 할 대상(비우면 자기 자신)

    public void Pickup()
    {
        Debug.Log($"[PICKUP] {itemId} 클릭됨 / this={gameObject.name}");

        GameManager.I.AddItem(itemId);

        if (objectToHide == null) objectToHide = gameObject;
        objectToHide.SetActive(false);
    }
}