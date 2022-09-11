public class Jumper : Enemy
{
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
        movementPattern.Add(new GridCoord(0, 1));
    }

    public override void TakeVehicleInTheWayAction()
    {
        yAdjustment = 3;
        commandStack.Enqueue(new MoveWithinGridCommand(FieldGrid.GetSingleGrid(GetCurrentGridPosition()).GetCornerPoint(0, 1)));
    }

    public override void TakeNoVehicleInTheWayAction()
    {
        if (yAdjustment == 3)
        {
            commandStack.Enqueue(new MoveWithinGridCommand(FieldGrid.GetSingleGrid(GetCurrentGridPosition()).GetCornerPoint(0, 1), yAdjustment));
        }
        yAdjustment = 0;
    }

    public override bool HaltMovementByVehicleInTheWay()
    {
        return false;
    }

    public override string GetName()
    {
        return "Jumper";
    }

    public override string GetDescription()
    {
        return "Movement Pattern: Moves 1 step forward per turn. <br> <br>" +
            "Vehicle in the way: It will jump on top of the vehicle. <br> <br>" +
            "Additional effects: It will move along with the vehicle's movement.";
    }
}