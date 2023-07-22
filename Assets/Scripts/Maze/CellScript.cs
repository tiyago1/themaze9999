using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CellScript : MonoBehaviour
{
    public List<Column> walls;
    public Column wallL;
    public Column wallR;
    public Column wallU;
    public Column wallD;

    public Column GetColumnWithDirection(Vector2 direction)
    {
        return walls.FirstOrDefault(wall => wall.Direction == direction);
    }
}

