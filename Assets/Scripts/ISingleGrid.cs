using System.Collections.Generic;
using UnityEngine;

public interface ISingleGrid
{
    void AddObject(GameObject gameobj);
    void RemoveObject(int gameobjID);
    Vector3 GetGridCentrePoint();
    int GetObjectCount();
    int GetUnitCount();
    List<string> GetUnitsTag();
    void RepositionObjects();
}