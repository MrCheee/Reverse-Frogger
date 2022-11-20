using System.Collections.Generic;
using UnityEngine;

public class FieldGrid
{
    public static int FieldLength { get; private set; }
    public static int FieldHeight { get; private set; }
    public static int FieldBuffer { get; private set; }
    public static int NumOfLanes { get; private set; }
    public static int DividerY { get; private set; }
    public static int SidewalkBottomY { get; private set; }
    public static int SidewalkTopY { get; private set; }
    public static int MinPlayFieldX { get; private set; }
    public static int MaxPlayFieldX { get; private set; }
    public static int MinPlayFieldY { get; private set; }
    public static int MaxPlayFieldY { get; private set; }

    private static readonly FieldGrid _instance = new FieldGrid();
    private SingleGrid[,] field;
    private static HashSet<GridCoord> gridsToReposition;

    private FieldGrid()
    {
        FieldLength = 17;
        FieldHeight = 17;
        FieldBuffer = 3;
        NumOfLanes = 4;

        field = new SingleGrid[FieldLength, FieldHeight];
        for (int i = 0; i < FieldLength; i++)
        {
            for (int j = 0; j < FieldHeight; j++)
            {
                field[i, j] = new SingleGrid(i, j, new Vector3(i * 5, 0, j * 5));
            }
        }
        DividerY = FieldBuffer + NumOfLanes + 1;
        SidewalkTopY = FieldHeight - 1 - FieldBuffer;
        SidewalkBottomY = FieldBuffer;

        MinPlayFieldX = FieldBuffer;
        MaxPlayFieldX = FieldLength - 1 - FieldBuffer;
        MinPlayFieldY = FieldBuffer;
        MaxPlayFieldY = FieldHeight - 1 - FieldBuffer;

        gridsToReposition = new HashSet<GridCoord>();
    }

    public static FieldGrid GetFieldGrid()
    {
        return _instance;
    }

    public static SingleGrid GetGrid(int x, int y)
    {
        if (!IsWithinField(x, y))
        {
            throw new AccessingFieldGridOutOfBoundsException();
        }
        return _instance.field[x, y];
    }

    public static SingleGrid GetGrid(GridCoord gridCoord)
    {
        if (!IsWithinField(gridCoord))
        {
            throw new AccessingFieldGridOutOfBoundsException();
        }
        return _instance.field[gridCoord.x, gridCoord.y];
    }

    public static bool IsWithinField(GridCoord gridCoord)
    {
        return IsWithinField(gridCoord.x, gridCoord.y);
    }

    public static bool IsWithinField(int x, int y)
    {
        return !(x < 0 || x >= FieldLength || y < 0 || y >= FieldHeight);
    }

    public static bool IsWithinPlayableField(GridCoord gridCoord)
    {
        return IsWithinPlayableField(gridCoord.x, gridCoord.y);
    }

    public static bool IsWithinPlayableField(int x, int y)
    {
        return !(x < MinPlayFieldX || x > MaxPlayFieldX || y < MinPlayFieldY || y >= MaxPlayFieldY);
    }

    public static void AddGridToReposition(GridCoord gridCoord)
    {
        gridsToReposition.Add(gridCoord);
    }

    public static void TriggerGridsRepositioning()
    {
        foreach (GridCoord coord in gridsToReposition)
        {
            _instance.field[coord.x, coord.y].RepositionObjects();
        }
        gridsToReposition.Clear();
    }

    public static GridCoord GetGridCoordFromWorldPosition(Vector3 worldPos)
    {
        // Return default value as 0, 0 grid if selection is out of valid zone
        if (worldPos.x < -2.5 || worldPos.x >= (FieldLength * 5 - 2.5) || worldPos.z < -2.5 || worldPos.z >= (FieldHeight * 5 - 2.5))
        {
            return new GridCoord(0, 0);
        }

        int grid_x = (int)Mathf.Floor((worldPos.x + 2.5f) / 5);
        int grid_y = (int)Mathf.Floor((worldPos.z + 2.5f) / 5);
        return new GridCoord(grid_x, grid_y);
    }

    public static bool IsTargetedGridInALane(int gridY)
    {
        return gridY != DividerY && gridY < SidewalkTopY && gridY > SidewalkBottomY;
    }

    public static bool IsWithinPlayableX(int gridX)
    {
        return gridX >= MinPlayFieldX && gridX <= MaxPlayFieldX;
    }

    public static bool IsVehicleInTheWay(GridCoord targetGrid)
    {
        return IsWithinField(targetGrid) && GetGrid(targetGrid).GetListOfUnitsGameObjectTag().Contains("Vehicle");
    }

    public static bool IsEnemyInTheWay(GridCoord targetGrid)
    {
        return IsWithinField(targetGrid) && GetGrid(targetGrid).GetListOfUnitsGameObjectTag().Contains("Enemy");
    }

    public static bool IsUnitOfTypeInTheWay(GridCoord targetGrid, string unitTag)
    {
        return IsWithinField(targetGrid) && GetGrid(targetGrid).GetListOfUnitsTag().Contains(unitTag);
    }
}
