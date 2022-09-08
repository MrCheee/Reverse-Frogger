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
        commandStack.Enqueue(new MoveWithinGridCommand(FieldGrid.GetSingleGrid(GetCurrentGridPosition()).GetCornerPoint(0, 1)));
    }

    public override void TakeNoVehicleInTheWayAction()
    {
        if (yAdjustment == -0.25f)
        {
            commandStack.Enqueue(new MoveWithinGridCommand(FieldGrid.GetSingleGrid(GetCurrentGridPosition()).GetCornerPoint(0, 1), yAdjustment));
        }
        yAdjustment = 0;
    }

    public override bool HaltMovementByVehicleInTheWay()
    {
        return false;
    }
}