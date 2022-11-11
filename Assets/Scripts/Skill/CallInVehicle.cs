using System.Collections.Generic;
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

        // Check for any natural spawned vehicles in that lane that has not entered, and delete it, such that the called in veh enters immediately
        List<Unit> existingVehsInGrid = FieldGrid.GetSingleGrid(targetGrid).GetListOfUnitsWithGameObjectTag("Vehicle");
        foreach (Unit veh in existingVehsInGrid)
        {
            if (Helper.IsEqualGridCoords(veh.GetCurrentHeadGridPosition(), targetGrid))
            {
                veh.DestroySelf();
            }
        }

        // If there is still a vehicle in the way, i.e. a truck or bus that has already entered playing field, then spawn behind it
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
            foreach (SpriteRenderer vehSprite in unit.GetComponentsInChildren<SpriteRenderer>())
            {
                vehSprite.flipY = true;
            }
            unit.transform.Rotate(Vector3.up, 180f);
        }

        unit.gameObject.SetActive(true);
    }

    public void UpdateGridCoordAction(GridCoord coord)
    {
        targetGrid = coord;
    }
}