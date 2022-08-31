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
}