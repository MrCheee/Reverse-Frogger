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
        if (ToSkipTurn()) yield break;

        TurnInProgress = true;
        PreTurnActions();
        int retries = 0;
        int vehicleCheck = 0;
        bool moved = false;
        bool quickExit = false;
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
                else   // If coast is clear, further checks
                {
                    if (IsShieldBlockingTheWay(nextMove, nextGrid)) break;  // If Shield is blocking, halt current and further movement
                    if (IsEnemyTypeInTheWay(nextGrid, "Brute"))  // If Brute in the way, check if movement will kill it. If yes, proceed as normal.
                    {
                        Unit brute = GetUnitInTheWay(nextGrid, "Brute");
                        if (IsBruteTooStrong(brute))  // Brute will always be too strong for motorbike
                        {
                            HandleBruteInTheWay(brute);  // Deal no damage to brute
                            break;
                        }
                    }
                    if (IsEnemyTypeInTheWay(nextGrid, "Bloat")) quickExit = true;
                    MoveUnitsOnTopOfVehicle(nextMove);
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
                if (quickExit) break;
                yield return new WaitForSeconds(0.2f);
            }
        }
        TurnInProgress = false;
        PostTurnActions();
    }

    // Motorbike will deal no damage to a brute
    public override void HandleBruteInTheWay(Unit brute)   
    {
        GridCoord currentGrid = GetCurrentHeadGridPosition();
        int right = currentGrid.y < dividerY ? -1 : 1;
        commandStack.Enqueue(new MoveWithinGridCommand(FieldGrid.GetSingleGrid(currentGrid).GetCornerPoint(right, 0)));
        commandStack.Enqueue(new MoveWithinGridCommand(FieldGrid.GetSingleGrid(currentGrid).GetCornerPoint(0, 0)));
    }

    public override bool IsBruteTooStrong(Unit brute)
    {
        return true;
    }

    protected override void SetAdditionalTag()
    {
        unitTag = "Knockback-able Vehicle";
    }

    public override string GetName()
    {
        return "Motorbike";
    }

    public override string GetDescription()
    {
        return "Size: 1 grid <br> <br>" +
            "Speed: <br>-Base speed of 1 step, Maximum speed of 3 steps. <br> <br>" +
            "Additional effects: <br>-It will weave inbetween lanes if there is a slower or disabled vehicle in front of it. " +
            "<br>-Can be displaced by strong enemies. Manual lane change possible.";
    }
}