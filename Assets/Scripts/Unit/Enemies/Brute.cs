using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Brute : Enemy
{
    protected static readonly int knockbackAP = Animator.StringToHash("Knockback");

    int[] noKnockbackYPos = new int[2] {
        FieldGrid.DividerY + 1,   // Right Lane 1 - no vehicle to hit
        FieldGrid.DividerY - FieldGrid.NumOfLanes  // Left Lane 4 - no vehicle to hit
    };
    int[] KnockbackBlockYPos = new int[2]
    {
        FieldGrid.DividerY + 2,   // Right Lane 2 - cannot hit vehicle to next lane (next lane is divider)
        FieldGrid.DividerY - FieldGrid.NumOfLanes + 1    // Left Lane 3 - cannot hit vehicle to next lane (next lane is sidewalk)
    };
    bool movementBlocked = true;

    protected override void SetUnitAttributes()
    {
        Health = 2;
        Damage = 4;
        chargePerTurn = 0;
        SpecialTag = "Brute";
    }

    protected override void SetMovementPattern()
    {
        movementPattern.Add(new GridCoord(0, direction));
    }

    protected override IEnumerator PreTurnActions()
    {
        if (HasCrossed || SkipTurn > 0 || Charging > 0)
        {
            TurnInProgress = false;
            yield break;
        }

        GridCoord nextMove = movementPattern[0];
        GridCoord nextGrid = Helper.AddGridCoords(_currentGridPosition, nextMove);
        if (FieldGrid.IsVehicleInTheWay(nextGrid))
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
            if (FieldGrid.GetGrid(targetGrid).IsUnitTagInGrid("Knockback-able Vehicle"))
            {
                bool knockbackBlocked = KnockbackBlockYPos.Contains(_currentGridPosition.y);
                bool vehicleBlockingDestination = FieldGrid.IsVehicleInTheWay(destinationGrid);
                bool bruteBlockingDestination = false;

                bool bruteAtDestination = FieldGrid.IsUnitOfTypeInTheWay(destinationGrid, "Brute");
                if (bruteAtDestination)
                {
                    var allBrutes = FieldGrid.GetGrid(destinationGrid).GetListOfUnitsWithTag("Brute").Select(x => x as Brute).ToList();
                    bruteBlockingDestination = allBrutes.Any(x => x.Health == 2);
                }

                animator.SetTrigger(knockbackAP);
                Unit veh = FieldGrid.GetGrid(targetGrid).GetUnitWithTag("Knockback-able Vehicle");

                if (!knockbackBlocked && !vehicleBlockingDestination && !bruteBlockingDestination)
                {
                    veh.IssueCommand(new MoveToTargetGridCommand(destinationGrid));
                    veh.gameObject.GetComponent<Vehicle>().MoveUnitsOnTopOfVehicle(new GridCoord(0, direction));
                }
                else
                {
                    veh.IssueCommand(new MoveWithinCurrentGridCommand(0, direction));
                    veh.IssueCommand(new MoveWithinCurrentGridCommand(0, 0));
                }
            }
        }
    }

    protected override void TakeVehicleInTheWayAction()
    {
        ExecuteConcussedMovement();
    }

    protected override bool HaltMovementByVehicleInTheWay()
    {
        return movementBlocked;
    }

    public override string GetName()
    {
        return "Minotaur";
    }

    public override string GetDescription()
    {
        return "Movement Pattern: <br>-Moves 1 step forward per turn. <br> <br>" +
            "Vehicle in the way: <br>-Will knock the vehicle forward into the next lane (if the vehicle is displaceable and the lane is clear) " +
            "and move forward, else it will stay in place. <br> <br> " +
            "Additional effects: <br>-Requires 2 hit to kill. 1st hit will stop the colliding vehicle in its track.";
    }
}