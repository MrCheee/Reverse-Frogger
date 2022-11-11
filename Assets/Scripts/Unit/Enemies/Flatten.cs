using System.Collections;

public class Flatten : Enemy
{
    protected override void SetUnitAttributes()
    {
        health = 1;
        damage = 1;
        chargePerTurn = 0;
    }

    public override void SetMovementPattern()
    {
        movementPattern.Add(new GridCoord(0, direction));
    }

    public override void TakeVehicleInTheWayAction()
    {
        yAdjustment = -2f;
        commandStack.Enqueue(new MoveWithinGridCommand(FieldGrid.GetSingleGrid(GetCurrentHeadGridPosition()).GetCornerPoint(0, direction)));
    }

    public override void TakeNoVehicleInTheWayAction()
    {
        if (yAdjustment == -2f)
        {
            animator.SetBool("Flatten", false);
            commandStack.Enqueue(new MoveWithinGridCommand(FieldGrid.GetSingleGrid(GetCurrentHeadGridPosition()).GetCornerPoint(0, direction), yAdjustment));
        }
        yAdjustment = 0;
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
            if (!animator.GetBool("Flatten"))
            {
                animator.SetTrigger("ToFlatten");
                animator.SetBool("Flatten", true);
            }
        }
        TurnInProgress = false;
        yield break;
    }

    public override bool HaltMovementByVehicleInTheWay()
    {
        return false;
    }

    public override string GetName()
    {
        return "Mutated Blood";
    }

    public override string GetDescription()
    {
        return "Movement Pattern: <br>-Moves 1 step forward per turn. <br> <br>" +
            "Vehicle in the way: <br>-It will flatten itself and move under the vehicle. <br> <br>" +
            "Additional effects: <br>-It will remain in its flattened state until it has moved out from under a vehicle.";
    }
}