using System.Linq;

public class Brute : Enemy
{
    int[] noKnockbackYPos = new int[4] {
        FieldGrid.GetDividerLaneNum() - 1,   // Left Lane 1 - no vehicle to hit
        FieldGrid.GetDividerLaneNum() - 2,   // Left Lane 2 - cannot hit vehicle to next lane (next lane is divider)
        FieldGrid.GetDividerLaneNum() + FieldGrid.GetNumberOfLanes(),  // Right Lane 1 - no vehicle to hit
        FieldGrid.GetDividerLaneNum() + FieldGrid.GetNumberOfLanes() - 1    // Right Lane 2 - cannot hit vehicle to next lane (next lane is sidewalk)
    };
    bool movementBlocked = true;

    protected override void SetAdditionalTag()
    {
        unitTag = "Brute";
    }

    protected override void SetHealthAndDamage()
    {
        health = 2;
        damage = 2;
    }
    
    public override void SetMovementPattern()
    {
        movementPattern.Add(new GridCoord(0, 1));
    }

    public override void TakeVehicleInTheWayAction()
    {
        movementBlocked = true;
        if (!noKnockbackYPos.Contains(_currentGridPosition.y))   // Brute is in a lane that can knockback vehicles
        {
            GridCoord targetGrid = new GridCoord(_currentGridPosition.x, _currentGridPosition.y + 1);
            GridCoord destinationGrid = new GridCoord(_currentGridPosition.x, _currentGridPosition.y + 2);

            // If vehicle in front is knockback-able, and there is no vehicle blocking its knockback, then knockback and move forward
            if (FieldGrid.GetSingleGrid(targetGrid).IsUnitTagInGrid("Knockback-able Vehicle"))
            {
                if (!Helper.IsVehicleInTheWay(destinationGrid))
                {
                    Unit veh = FieldGrid.GetSingleGrid(targetGrid).GetUnitWithTag("Knockback-able Vehicle");
                    veh.IssueCommand(new MoveToGridCommand(new GridCoord(0, 1)));
                    movementBlocked = false;
                }
            }
        }
        if (movementBlocked)
        {
            GridCoord currentGrid = GetCurrentGridPosition();
            commandStack.Enqueue(new MoveWithinGridCommand(FieldGrid.GetSingleGrid(currentGrid).GetCornerPoint(0, 1)));
            commandStack.Enqueue(new MoveWithinGridCommand(FieldGrid.GetSingleGrid(currentGrid).GetCornerPoint(0, 0)));
        }
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
        return "Movement Pattern: Moves 1 step forward per turn. <br> <br>" +
            "Vehicle in the way: Will knock the vehicle forward into the next lane (if possible) and move forward, " +
            "else it will stay in place. <br> <br> " + 
            "Additional effects: Requires 2 hit to kill. 1st hit will stop the colliding vehicle in its track.";
    }
}