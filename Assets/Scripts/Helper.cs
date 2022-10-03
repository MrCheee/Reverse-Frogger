using UnityEngine;

public class Helper
{
    public static float vectorDistanceIgnoringYAxis(Vector3 v1, Vector3 v2)
    {
        float xDiff = v1.x - v2.x;
        float zDiff = v1.z - v2.z;
        return Mathf.Sqrt((xDiff * xDiff) + (zDiff * zDiff));
    }

    public static GridCoord AddGridCoords(GridCoord current, GridCoord next)
    {
        return new GridCoord(current.x + next.x, current.y + next.y);
    }

    public static bool IsEqualGridCoords(GridCoord first, GridCoord second)
    {
        return first.x == second.x && first.y == second.y;
    }

    public static bool IsVehicleInTheWay(GridCoord targetGrid)
    {
        return FieldGrid.IsWithinField(targetGrid) && FieldGrid.GetSingleGrid(targetGrid).GetListOfUnitsGameObjectTag().Contains("Vehicle");
    }

    public static bool IsUnitOfTypeInTheWay(GridCoord targetGrid, string unitTag)
    {
        return FieldGrid.IsWithinField(targetGrid) && FieldGrid.GetSingleGrid(targetGrid).GetListOfUnitsGameObjectTag().Contains(unitTag);
    }

    public static Vector2 ScreenPointToWorldPosition(Vector3 screenPos, float scaleFactor = 1)
    {
        float h = Screen.height;
        float w = Screen.width;
        float x = screenPos.x - (w / 2);
        float y = screenPos.y - (h / 2);
        float s = scaleFactor;
        return new Vector2(x, y) / s;
    }
}