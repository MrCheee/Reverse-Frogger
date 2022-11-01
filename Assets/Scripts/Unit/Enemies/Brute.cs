using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Brute : Enemy
{
    int[] noKnockbackYPos = new int[4] {
        FieldGrid.GetDividerLaneNum() + 1,   // Right Lane 1 - no vehicle to hit
        FieldGrid.GetDividerLaneNum() + 2,   // Right Lane 2 - cannot hit vehicle to next lane (next lane is divider)
        FieldGrid.GetDividerLaneNum() - FieldGrid.GetNumberOfLanes(),  // Left Lane 4 - no vehicle to hit
        FieldGrid.GetDividerLaneNum() - FieldGrid.GetNumberOfLanes() + 1    // Left Lane 3 - cannot hit vehicle to next lane (next lane is sidewalk)
    };
    bool movementBlocked = true;

    protected override void SetAdditionalTag()
    {
        unitTag = "Brute";
    }

    protected override void SetHealthAndDamage()
    {
        health = 2;
        damage = 4;
    }
    
    public override void SetMovementPattern()
    {
        movementPattern.Add(new GridCoord(0, direction));
    }

    public override IEnumerator PreTurnActions()
    {
        if (Crossed || skipTurn > 0 || charging > 0)
        {
            TurnInProgress = false;
            yield break;
        }

        GridCoord nextMove = movementPattern[0];
        GridCoord nextGrid = Helper.AddGridCoords(_currentGridPosition, nextMove);
        if (Helper.IsVehicleInTheWay(nextGrid))
        {
            KnockbackVehicleInTheWay();
        }

        TurnInProgress = false;
        yield break;
    }

    private void KnockbackVehicleInTheWay()
    {
        if (!noKnockbackYPos.Contains(_currentGridPosition.y))   // Brute is in a lane that can knockback vehicles
        {
            GridCoord targetGrid = new GridCoord(_currentGridPosition.x, _currentGridPosition.y + direction);
            GridCoord destinationGrid = new GridCoord(_currentGridPosition.x, _currentGridPosition.y + direction * 2);
        
            // If vehicle in front is knockback-able, and there is no vehicle blocking its knockback, then knockback and move forward
            if (FieldGrid.GetSingleGrid(targetGrid).IsUnitTagInGrid("Knockback-able Vehicle"))
            {
                bool vehicleBlockingDestination = Helper.IsVehicleInTheWay(destinationGrid);
                bool bruteBlockingDestination = false;

                bool bruteAtDestination = Helper.IsUnitOfTypeInTheWay(destinationGrid, "Brute");
                if (bruteAtDestination)
                {
                    List<Unit> allBrutes = FieldGrid.GetSingleGrid(destinationGrid).GetListOfUnitsWithTag("Brute");
                    bruteBlockingDestination = allBrutes.Any(x => x.GetHealth() == 2);
                }

                if (!vehicleBlockingDestination && !bruteBlockingDestination)
                {
                    Unit veh = FieldGrid.GetSingleGrid(targetGrid).GetUnitWithTag("Knockback-able Vehicle");
                    veh.IssueCommand(new MoveToTargetGridCommand(destinationGrid));
                }
            }
        }
    }

    public override void TakeVehicleInTheWayAction()
    {
        //movementBlocked = true;
        //if (!noKnockbackYPos.Contains(_currentGridPosition.y))   // Brute is in a lane that can knockback vehicles
        //{
        //    GridCoord targetGrid = new GridCoord(_currentGridPosition.x, _currentGridPosition.y + direction);
        //    GridCoord destinationGrid = new GridCoord(_currentGridPosition.x, _currentGridPosition.y + direction * 2);
        //
        //    // If vehicle in front is knockback-able, and there is no vehicle blocking its knockback, then knockback and move forward
        //    if (FieldGrid.GetSingleGrid(targetGrid).IsUnitTagInGrid("Knockback-able Vehicle"))
        //    {
        //        if (!Helper.IsVehicleInTheWay(destinationGrid))
        //        {
        //            Unit veh = FieldGrid.GetSingleGrid(targetGrid).GetUnitWithTag("Knockback-able Vehicle");
        //            veh.IssueCommand(new MoveToGridCommand(new GridCoord(0, direction)));
        //            movementBlocked = false;
        //        }
        //    }
        //}
        //if (movementBlocked)
        //{
        //    ExecuteConcussedMovement();
        //}
        ExecuteConcussedMovement();
    }

    public override bool HaltMovementByVehicleInTheWay()
    {
        return movementBlocked;
    }

    public override string GetName()
    {
        return "Brute";
    }

    public override string GetDescription()
    {
        return "Movement Pattern: <br>-Moves 1 step forward per turn. <br> <br>" +
            "Vehicle in the way: <br>-Will knock the vehicle forward into the next lane (if the vehicle is displaceable and the lane is clear) " +
            "and move forward, else it will stay in place. <br> <br> " +
            "Additional effects: <br>-Requires 2 hit to kill. 1st hit will stop the colliding vehicle in its track.";
    }
}