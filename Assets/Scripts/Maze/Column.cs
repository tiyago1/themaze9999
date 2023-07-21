using UnityEngine;


public class Column : MonoBehaviour
{
    public bool IsActive;

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
        this.gameObject.SetActive(IsActive);
    }
}