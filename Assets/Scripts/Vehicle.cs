using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Vehicle : Unit
{
    public int SpeedAddition { get; set; }

    public override void PreTurnActions()
    {
        return;
    }
    public override void PostTurnActions()
    {
        return;
    }

    public override IEnumerator TakeTurn()
    {
        PreTurnActions();
        int retries = 0;
        Queue<GridCoord> moveQueue = new Queue<GridCoord>(movementPattern);
        GridCoord nextMove = moveQueue.Peek();
        GridCoord nextGrid = Helper.AddGridCoords(CurrentGridPosition, nextMove);
        while (moveQueue.Count > 0)
        {
            // If vehicle cannot proceed for a full second, halt current and further movement
            if (retries > 10) break;

            // Wait until all commands have been completed by the unit before issuing next move command
            if (_currentCommand != null)
            {
                yield return new WaitForSeconds(0.1f);
            }
            // If vehicle is in the way, wait for 0.1s and retry
            if (Helper.IsVehicleInTheWay(nextGrid))
            {
                retries += 1;
                yield return new WaitForSeconds(0.1f);
            }
            else   // If coast is clear, issue the next movement command
            {
                GiveMovementCommand(nextMove);
                moveQueue.Dequeue();
                if (moveQueue.Count > 0)
                {
                    nextMove = moveQueue.Peek();
                    nextGrid = Helper.AddGridCoords(nextGrid, nextMove);
                }
                yield return new WaitForSeconds(0.1f);
            }
        }
        TurnInProgress = false;
        PostTurnActions();
    }

    public override void CheckConditionsToDestroy()
    {
        if (HasReachedEndOfRoad())
        {
            DestroySelf();
        }
    }

    public bool HasReachedEndOfRoad()
    {
        if (CurrentGridPosition.y < 4)
        {
            return CurrentGridPosition.x == 0;
        }
        else
        {
            return CurrentGridPosition.x == 10;
        }
    }

    public bool IsBruteInTheWay(GridCoord targetGrid)
    {
        return FieldGrid.IsWithinField(targetGrid) && FieldGrid.GetSingleGrid(targetGrid).GetUnitsTag().Contains("Brute");
    }
    public void reverseMotion()
    {
        movementPattern = movementPattern.Select(x => new GridCoord(x.x * -1, x.y)).ToList();
    }

}