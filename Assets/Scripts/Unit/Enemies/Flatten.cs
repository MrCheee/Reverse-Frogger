public class Flatten : Enemy
{
    protected override void SetHealthAndDamage()
    {
        health = 1;
        damage = 1;
    }

    public override void SetMovementPattern()
    {
        movementPattern.Add(new GridCoord(0, 1));
    }

    public override void TakeVehicleInTheWayAction()
    {
        yAdjustment = -0.25f;
        commandStack.Enqueue(new MoveWithinGridCommand(FieldGrid.GetSingleGrid(GetCurrentHeadGridPosition()).GetCornerPoint(0, 1)));
    }

    public override void TakeNoVehicleInTheWayAction()
    {
        if (yAdjustment == -0.25f)
        {
            commandStack.Enqueue(new MoveWithinGridCommand(FieldGrid.GetSingleGrid(GetCurrentHeadGridPosition()).GetCornerPoint(0, 1), yAdjustment));
        }
        yAdjustment = 0;
    }

    public override bool HaltMovementByVehicleInTheWay()
    {
        return false;
    }

    public override string GetName()
    {
        return "Flatten";
    }

    public override string GetDescription()
    {
        return "Movement Pattern: Moves 1 step forward per turn. <br> <br>" +
            "Vehicle in the way: It will flatten itself and move under the vehicle. <br> <br>" +
            "Additional effects: It will remain in its flattened state until it has moved out from under the vehicle.";
    }
}