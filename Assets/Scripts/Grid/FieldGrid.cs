using System.Collections.Generic;
using UnityEngine;

public class FieldGrid
{
    private static int fieldLength = 17;
    private static int fieldHeight = 18;
    private static int fieldBuffer = 3;
    private static int numOfLanes = 4;
    private static int dividerY;
    private static List<GridCoord> gridsToReposition = new List<GridCoord>();
    private SingleGrid[,] field = new SingleGrid[fieldLength, fieldHeight];

    private static readonly FieldGrid _instance = new FieldGrid();

    private FieldGrid()
    {
        for (int i = 0; i < fieldLength; i++)
        {
            for (int j = 0; j < fieldHeight; j++)
            {
                field[i, j] = new SingleGrid(i, j, new Vector3(i * 5, 0, j * 5));
            }
        }
        dividerY = fieldBuffer + numOfLanes + 1;
    }

    public static FieldGrid GetFieldGrid()
    {
        return _instance;
    }

    public static SingleGrid GetSingleGrid(int x, int y)
    {
        if (x < 0 || x >= fieldLength || y < 0 || y >= fieldHeight)
        {
            throw new AccessingFieldGridOutOfBoundsException();
        }
        return _instance.field[x, y];
    }

    public static SingleGrid GetSingleGrid(GridCoord gridCoord)
    {
        return _instance.field[gridCoord.x, gridCoord.y];
    }

    public static bool IsWithinField(GridCoord gridCoord)
    {
        return !(gridCoord.x < 0 || gridCoord.x >= fieldLength || gridCoord.y < 0 || gridCoord.y >= fieldHeight);
    }

    public static int GetMaxHeight()
    {
        return fieldHeight;
    }

    public static int GetMaxLength()
    {
        return fieldLength;
    }

    public static int GetFieldBuffer()
    {
        return fieldBuffer;
    }

    public static int GetNumberOfLanes()
    {
        return numOfLanes;
    }

    public static int GetDividerLaneNum()
    {
        return dividerY;
    }

    public static void AddGridToReposition(GridCoord gridCoord)
    {
        gridsToReposition.Add(gridCoord);
    }

    public static void TriggerGridsRepositioning()
    {
        foreach(GridCoord coord in gridsToReposition)
        {
            _instance.field[coord.x, coord.y].RepositionObjects();
        }
        gridsToReposition.Clear();
    }
}
