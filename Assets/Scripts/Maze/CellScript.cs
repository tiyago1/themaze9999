using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum CellType
{
    Empty,
    Fog
}

public class CellScript : MonoBehaviour
{
    public List<Column> walls;
    public Column wallL;
    public Column wallR;
    public Column wallU;
    public Column wallD;

    public SpriteRenderer CellRenderer;
    public List<Color> Colors;

    public Column GetColumnWithDirection(Vector2 direction)
    {
        return walls.FirstOrDefault(wall => wall.Direction == direction);
    }

    public void SetType(CellType cellType)
    {
        CellRenderer.color = Colors[(int) cellType];
    }
}

