using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : Unit
{
    private List<Vehicle> Resist;

    public override void TakeTurn()
    {
        if (HasCrossedTheRoad()) { return; }

        GridCoord projectedGridPos = currentGridPosition;
        foreach (GridCoord nextMove in movementPattern)
        {
            GridCoord nextGrid = Helper.AddGridCoords(projectedGridPos, nextMove);
            if (IsVehicleInTheWay(nextGrid))
            {
                break;
            }
            
            GiveMovementCommand(projectedGridPos, nextMove);
            projectedGridPos = nextGrid;
        }
    }

    public override void CheckConditionsToDestroy()
    {
        return;
    }

    public bool HasCrossedTheRoad()
    {
        return currentGridPosition.y == 8;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Hit by a car! @({currentGridPosition.x}, {currentGridPosition.y})...");
        DestroySelf();
    }
}