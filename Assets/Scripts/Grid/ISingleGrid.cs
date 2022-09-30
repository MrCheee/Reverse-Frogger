using System.Collections.Generic;
using UnityEngine;

public interface ISingleGrid
{
    void AddObject(GameObject gameobj);
    void RemoveObject(int gameobjID);
    Vector3 GetGridCentrePoint();
    GridCoord GetGridCoord();
    int GetObjectCount();
    int GetUnitCount();
    List<string> GetListOfUnitsGameObjectTag();
    void RepositionObjects();
}