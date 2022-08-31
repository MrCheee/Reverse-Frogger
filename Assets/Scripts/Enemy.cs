using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : Unit
{
    private List<Vehicle> Resist;

    public override void PreTurnActions()
    {
        return;
    }
    public override void PostTurnActions()
    {
        return;
    }

    // Turn ends when enemy has completed one cycle of its movement pattern
    // Or it is unable to complete its movement any further
    public override IEnumerator TakeTurn()
    {
        PreTurnActions();
        Queue<GridCoord> moveQueue = new Queue<GridCoord>(movementPattern);
        GridCoord nextMove = moveQueue.Peek();
        GridCoord nextGrid = Helper.AddGridCoords(CurrentGridPosition, nextMove);
        while (moveQueue.Count > 0)
        {
            if (HasCrossedTheRoad()) break;
            // Wait until all commands have been completed by the unit before issuing next move command
            if (commandStack.Count != 0)
            {
                yield return new WaitForSeconds(0.1f);
            }
            // If vehicle is in the way, cancel current movement and break out from all further movement
            if (Helper.IsVehicleInTheWay(nextGrid))
            {
                break;
            } 
            else   // If no vehicle, issue the next movement command
            {
                //Debug.Log($"Issue Move command of ({nextMove.x}, {nextMove.y}) to ({nextGrid.x}, {nextGrid.y})");
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
        return;
    }

    public bool HasCrossedTheRoad()
    {
        return CurrentGridPosition.y == 8;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Hit by a car! @({CurrentGridPosition.x}, {CurrentGridPosition.y})...");
        DestroySelf();
    }
}