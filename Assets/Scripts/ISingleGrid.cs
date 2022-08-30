using UnityEngine;

public interface ISingleGrid
{
    void AddUnit(Unit gameobj);
    Vector3 GetGridPoint();
    void RepositionUnits();
    public int GetUnitCount();
    public string GetUnitsTag();
}