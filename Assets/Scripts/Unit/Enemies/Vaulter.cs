using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vaulter : Enemy
{
    bool vaultAvailable = true;
    bool vaultReady = false;

    protected override void SetHealthAndDamage()
    {
        health = 1;
        damage = 1;
    }

    protected override void SetAdditionalTag()
    {
        unitTag = "Roof-Ready";
    }

    public override void SetMovementPattern()
    {
        movementPattern.Add(new GridCoord(0, direction));
    }

    public override IEnumerator PreTurnActions()
    {
        GridCoord nextMove = movementPattern[0];
        GridCoord nextGrid = Helper.AddGridCoords(_currentGridPosition, nextMove);

        if (!vaultReady)
        {
            if (!Helper.IsVehicleInTheWay(nextGrid))
            {
                commandStack.Enqueue(new RotateCommand(Vector3.left, 90, 90));
                vaultReady = true;
            }
        }

        TurnInProgress = false;
        yield break;
    }

    public override IEnumerator TakeTurn()
    {
        float retryInterval = 0.2f;
        bool vaulted = false;

        if (Crossed)
        {
            TurnInProgress = false;
            yield break;
        }
        if (ToSkipTurn()) yield break;
        if (StillChargingUp()) yield break;

        PrepareMovement(out Queue<GridCoord> moveQueue, out GridCoord nextMove, out GridCoord nextGrid);
        GridCoord nextnextGrid;

        if (!vaultReady)
        {
            if (!Helper.IsVehicleInTheWay(nextGrid))
            {
                transform.Rotate(new Vector3(-90, 0, 0));
                vaultReady = true;
            }
        }

        while (moveQueue.Count > 0)
        {
            // Wait until all commands have been completed by the unit before issuing next move command
            if (!CheckIfCompletedPreviousMovement()) yield return new WaitForSeconds(retryInterval);

            nextnextGrid = Helper.AddGridCoords(nextGrid, nextMove);
            if (vaultReady && vaultAvailable)
            {
                // Check if theres a vehicle 2 grids away from it, if so, then perform a vault and end turn
                if (Helper.IsVehicleInTheWay(nextnextGrid))
                {
                    TakeVehicleInNextNextGridAction();
                    vaulted = true;
                }
            }

            // If not vaulted, perform usual next grid vehicle check and move accordingly
            if (!vaulted)
            {
                if (Helper.IsVehicleInTheWay(nextGrid))
                {
                    TakeVehicleInNextGridAction();
                    if (HaltMovementByVehicleInTheWay()) break;
                }
                else
                {
                    TakeNoVehicleInTheWayAction();
                }
            }
            
            // If not break away by vehicle in the way, issue the next movement command
            //Debug.Log($"Issue Move command of ({nextMove.x}, {nextMove.y}) to ({nextGrid.x}, {nextGrid.y})");
            GiveMovementCommand(nextMove);

            // Halt further movement when enemy has crossed the road
            if (HasCrossedTheRoad(nextGrid))
            {
                TriggerDamageOnPlayer();
                MarkedAsCrossed();
                break;
            }

            (nextMove, nextGrid) = GetNextMovementAction(moveQueue, nextMove, nextGrid);

            yield return new WaitForSeconds(0.2f);
        }
        TurnInProgress = false;
    }

    public void TakeVehicleInNextGridAction()
    {
        GridCoord currentGrid = GetCurrentHeadGridPosition();
        if (yAdjustment == 0)  // If not on another vehicle, then give "knocked" movement and skip turn
        {
            skipTurn = 1;
            ExecuteConcussedMovement();
        }
    }

    // If vehicle is in the way 2 grids away and vaulting is executed, vaulter will fly from current position and land 3 grids forward.
    public void TakeVehicleInNextNextGridAction()
    {
        GridCoord currentGrid = GetCurrentHeadGridPosition();
        GridCoord nextGrid = new GridCoord(currentGrid.x, currentGrid.y + direction);
        GridCoord nextnextGrid = new GridCoord(currentGrid.x, currentGrid.y + direction * 2);
        GridCoord landingGrid = new GridCoord(currentGrid.x, currentGrid.y + direction * 3);
        
        GetComponentInChildren<VaultTrigger>().DestroySelf();

        if (Helper.IsVehicleInTheWay(landingGrid))
        {
            yAdjustment = 3;
        }

        // Flying over 1st grid
        commandStack.Enqueue(new MoveWithinGridCommand(FieldGrid.GetSingleGrid(nextGrid).GetCornerPoint(0, -direction), 3));
        commandStack.Enqueue(new MoveWithinGridCommand(FieldGrid.GetSingleGrid(nextGrid).GetCornerPoint(0, direction), 3));

        // Flying over 2nd grid
        commandStack.Enqueue(new MoveWithinGridCommand(FieldGrid.GetSingleGrid(nextnextGrid).GetCornerPoint(0, direction), 3));

        RemoveFromFieldGridPosition();
        AddToFieldGridPosition(nextnextGrid);
        vaultAvailable = false;
    }

    public override void TakeNoVehicleInTheWayAction()
    {
        if (yAdjustment == 3)
        {
            commandStack.Enqueue(new MoveWithinGridCommand(FieldGrid.GetSingleGrid(GetCurrentHeadGridPosition()).GetCornerPoint(0, direction), yAdjustment));
        }
        yAdjustment = 0;
    }

    public override bool HaltMovementByVehicleInTheWay()
    {
        if (yAdjustment == 3)   // and landed on another vehicle
        {
            return false;       // then movement is not halted, can jump on top of next vehicle in the way
        }
        else
        {
            return true;        // If not on another vehicle, then no move.
        }
    }

    public void VaultHit()
    {
        skipTurn = 1;
        vaultAvailable = false;
    }

    public override string GetName()
    {
        return "Vaulter";
    }

    public override string GetDescription()
    {
        return "Movement Pattern: <br>-Moves 1 step forward per turn. <br> <br>" +
            "Vehicle in the way: <br>-If vault pole is available, it will vault over the vehicle and move forward 3 steps. " +
            "<br>-If no pole is available, it will run into the vehicle and becomes stunned for 1 turn. <br> <br> " +
            "Additional effects: <br>-It begins with its vault held skywards and will be unable to vault until it can bring its pole down. " +
            "<br>-It can only vault once, thereafter losing its pole. <br>-The pole extends one lane in front of it, and " +
            "a vehicle can run into it, destroying it and stunning the vaulter for 1 turn." +
            "<br>-After vaulting, it may land on top of a vehicle. While on top, it will move along with the vehicle and can hop onto another vehicle.";
    }
}