using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Motorbike : Vehicle
{
    protected override void SetUpSize()
    {
        size = 1;
        maxSpeed = 3;
    }

    public override void SetMovementPattern()
    {
        movementPattern.Add(new GridCoord(1, 0));
    }

    public override IEnumerator TakeTurn()
    {
        TurnInProgress = true;
        PreTurnActions();
        int retries = 0;
        int vehicleCheck = 0;
        bool moved = false;
        Queue<GridCoord> moveQueue = new Queue<GridCoord>(movementPattern);
        moveQueue = UpdateMovementWithLaneSpeed(moveQueue);
        GridCoord nextMove = moveQueue.Peek();
        GridCoord nextGrid = Helper.AddGridCoords(_currentGridPosition[0], nextMove);
        while (moveQueue.Count > 0)
        {
            // If vehicle cannot proceed for a full second, halt current and further movement
            if (retries > 5) break;

            // Wait until all commands have been completed by the unit before issuing next move command
            if (_currentCommand != null)
            {
                yield return new WaitForSeconds(0.2f);
            }
            else
            {
                // If vehicle is in the way, check again in 0.5s and move accordingly
                if (Helper.IsVehicleInTheWay(nextGrid))
                {
                    vehicleCheck += 1;
                    if (vehicleCheck > 3)
                    {
                        commandStack.Enqueue(new MoveToGridInbetweenCommand(nextMove));
                        moved = true;
                    }
                }
                else   // If coast is clear, issue the next movement command
                {
                    commandStack.Enqueue(new MoveToGridCommand(nextMove));
                    moved = true;
                }
                
                if (moved)
                {
                    moved = false;
                    retries = 0;
                    vehicleCheck = 0;
                    moveQueue.Dequeue();
                    if (moveQueue.Count > 0)
                    {
                        nextMove = moveQueue.Peek();
                        nextGrid = Helper.AddGridCoords(nextGrid, nextMove);
                    }
                }
                yield return new WaitForSeconds(0.2f);
            }
        }
        TurnInProgress = false;
        PostTurnActions();
    }
}