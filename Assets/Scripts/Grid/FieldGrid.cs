using System.Collections.Generic;
using UnityEngine;

public class FieldGrid
{
    private static int fieldLength = 17;
    private static int fieldHeight = 17;
    private static int fieldBuffer = 3;
    private static int numOfLanes = 4;

    private static int dividerY;
    private static int minPlayFieldX;
    private static int maxPlayFieldX;
    private static int minPlayFieldY;
    private static int maxPlayFieldY;
    private static int sidewalkBottomY;
    private static int sidewalkTopY;

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
        sidewalkTopY = fieldHeight - 1 - fieldBuffer;
        sidewalkBottomY = fieldBuffer;

        minPlayFieldX = fieldBuffer;
        maxPlayFieldX = fieldLength - 1 - fieldBuffer;
        minPlayFieldY = fieldBuffer;
        maxPlayFieldY = fieldHeight - 1 - fieldBuffer;
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
        if (gridCoord.x < 0 || gridCoord.x >= fieldLength || gridCoord.y < 0 || gridCoord.y >= fieldHeight)
        {
            throw new AccessingFieldGridOutOfBoundsException();
        }
        return _instance.field[gridCoord.x, gridCoord.y];
    }

    public static bool IsWithinField(GridCoord gridCoord)
    {
        return !(gridCoord.x < 0 || gridCoord.x >= fieldLength || gridCoord.y < 0 || gridCoord.y >= fieldHeight);
    }

    public static bool IsWithinPlayableField(GridCoord gridCoord)
    {
        return !(gridCoord.x < fieldBuffer || gridCoord.x >= fieldLength - fieldBuffer || gridCoord.y < fieldBuffer || gridCoord.y >= fieldHeight - fieldBuffer);
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

    public static int GetTopSidewalkLaneNum()
    {
        return sidewalkTopY;
    }

    public static int GetBottomSidewalkLaneNum()
    {
        return sidewalkBottomY;
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

    public static GridCoord GetGridCoordFromWorldPosition(Vector3 worldPos)
    {
        // Return default value as 0, 0 grid if selection is out of valid zone
        if (worldPos.x < -2.5 || worldPos.x >= (fieldLength * 5 - 2.5) || worldPos.z < -2.5 || worldPos.z >= (fieldHeight * 5 - 2.5))
        {
            return new GridCoord(0, 0);
        }

        int grid_x = (int)Mathf.Floor((worldPos.x + 2.5f) / 5);
        int grid_y = (int)Mathf.Floor((worldPos.z + 2.5f) / 5);
        return new GridCoord(grid_x, grid_y);
    }
}
