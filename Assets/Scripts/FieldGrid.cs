using UnityEngine;

public class FieldGrid
{
    private int fieldLength = 11;
    private int fieldHeight = 9;
    private SingleGrid[,] field = new SingleGrid[11,9];

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
        return _instance.field[x, y];
    }
}