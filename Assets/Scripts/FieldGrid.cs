using UnityEngine;

public class FieldGrid
{
    private static int fieldLength = 13;
    private static int fieldHeight = 11;
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
        return !(gridCoord.x < 0 || gridCoord.x >= 11 || gridCoord.y < 0 || gridCoord.y >= 9);
    }
}
