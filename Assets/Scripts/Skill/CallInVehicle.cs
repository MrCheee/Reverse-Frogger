using System.Collections.Generic;
using UnityEngine;

public class CallInVehicle : ISkill
{
    public Unit TargetUnit { get; set; }
    public GridCoord TargetGrid { get; set; }

    public CallInVehicle(Unit target)
    {
        TargetUnit = target;
    }

    public void Execute()
    {
        bool isLeftMovingLane = TargetGrid.y < FieldGrid.DividerY;

        // Check for any natural spawned vehicles in that lane that has not entered, and delete it, such that the called in veh enters immediately
        List<Unit> existingVehsInGrid = FieldGrid.GetGrid(TargetGrid).GetListOfUnitsWithGameObjectTag("Vehicle");
        foreach (Unit veh in existingVehsInGrid)
        {
            if (Helper.IsEqualGridCoords(veh.GetCurrentHeadGridPosition(), TargetGrid))
            {
                veh.TakeDamage(99);
            }
        }

        // If there is still a vehicle in the way, i.e. a truck or bus that has already entered playing field, then spawn behind it
        if (FieldGrid.IsVehicleInTheWay(TargetGrid))
        {
            if (isLeftMovingLane)
            {
                TargetGrid = new GridCoord(TargetGrid.x + 1, TargetGrid.y);
            }
            else
            {
                TargetGrid = new GridCoord(TargetGrid.x - 1, TargetGrid.y);
            }
        }

        TargetUnit.gameObject.transform.position = FieldGrid.GetGrid(TargetGrid).GetGridCentrePoint();
        TargetUnit.GetComponent<Vehicle>().AddToFieldGridPosition(TargetGrid);

        if (isLeftMovingLane)
        {
            TargetUnit.GetComponent<Vehicle>().ReverseMotion();
        }
        else
        {
            foreach (SpriteRenderer vehSprite in TargetUnit.GetComponentsInChildren<SpriteRenderer>())
            {
                vehSprite.flipY = true;
            }
            TargetUnit.transform.Rotate(Vector3.up, 180f);
        }

        TargetUnit.gameObject.SetActive(true);
    }

    public void UpdateGridCoordAction(GridCoord coord)
    {
        TargetGrid = coord;
    }
}