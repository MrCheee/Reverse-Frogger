using UnityEngine;

public class CallInVehicle : ISkill
{
    public GridCoord targetGrid { get; set; }
    public Unit unit { get; set; }

    public CallInVehicle(Unit target)
    {
        unit = target;
    }

    public void Execute()
    {
        bool isLeftMovingLane = targetGrid.y < FieldGrid.GetDividerLaneNum();
        if (Helper.IsVehicleInTheWay(targetGrid))
        {
            if (isLeftMovingLane)
            {
                targetGrid = new GridCoord(targetGrid.x + 1, targetGrid.y);
            }
            else
            {
                targetGrid = new GridCoord(targetGrid.x - 1, targetGrid.y);
            }
        }

        unit.gameObject.transform.position = FieldGrid.GetSingleGrid(targetGrid).GetGridCentrePoint();
        unit.GetComponent<Vehicle>().AddToFieldGridPosition(targetGrid);

        if (isLeftMovingLane)
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