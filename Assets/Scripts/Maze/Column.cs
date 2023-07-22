using System.Collections.Generic;
using UnityEngine;

public class Column : MonoBehaviour
{
    public Vector2 Direction;
    public bool IsActive;

    public List<Color> Colors;
    public SpriteRenderer CellRenderer;

    private void Awake()
    {
        SetActive(true);
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
        var type = isActive ? CellType.Empty : CellType.Fog;
        CellRenderer.color = Colors[(int)type];
        CellRenderer.sortingOrder = isActive?5:-1;
        // this.gameObject.SetActive(IsActive);
    }
}