using UnityEngine;

public class Column : MonoBehaviour
{
    public Vector2 Direction;
    public bool IsActive;

    private void Awake()
    {
        SetActive(true);
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
        this.gameObject.SetActive(IsActive);
    }
}