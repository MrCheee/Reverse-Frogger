using UnityEngine;

public class AirDropVehicle : ISkill
{
    public GridCoord targetGrid { get; set; }
    public Unit unit { get; set; }

    public AirDropVehicle(Unit target)
    {
        unit = target;
    }

    public void Execute()
    {
        unit.gameObject.transform.position = FieldGrid.GetSingleGrid(targetGrid).GetGridCentrePoint();
        unit.GetComponent<Vehicle>().AddToFieldGridPosition(targetGrid);
        if (targetGrid.y < FieldGrid.GetDividerLaneNum())
        {
            unit.GetComponent<Vehicle>().ReverseMotion();
        }
        else
        {
            unit.transform.Rotate(Vector3.up, 180f);
        }
        unit.gameObject.SetActive(true);
    }

    public void UpdateGridCoordAction(GridCoord coord)
    {
        targetGrid = coord;
    }
}