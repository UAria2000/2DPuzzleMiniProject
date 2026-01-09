using UnityEngine;

public class CloseTarget : MonoBehaviour
{
    public GameObject target; // 닫을 이미지(패널)

    public void Close()
    {
        if (target != null)
            target.SetActive(false);
    }

    public void Open()
    {
        if (target != null)
            target.SetActive(true);
    }
}