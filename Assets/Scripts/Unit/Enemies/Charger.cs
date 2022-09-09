public class Charger : Enemy
{
    protected override void SetHealthAndDamage()
    {
        health = 1;
        damage = 1;
    }

    protected override void SetChargePerTurn()
    {
        chargePerTurn = 3;
        charging = chargePerTurn;
    }

    public override void SetMovementPattern()
    {
        int totalLength = FieldGrid.GetNumberOfLanes() + 1;  // Dash half of the map, start to divider, then divider to end
        for (int i = 0; i < totalLength; i++)
        {
            movementPattern.Add(new GridCoord(0, 1));
        }
    }

    public override void TakeVehicleInTheWayAction()
    {
        skipTurn = 1;

        GridCoord currentGrid = GetCurrentGridPosition();
        commandStack.Enqueue(new MoveWithinGridCommand(FieldGrid.GetSingleGrid(currentGrid).GetCornerPoint(0, 1)));
        commandStack.Enqueue(new MoveWithinGridCommand(FieldGrid.GetSingleGrid(currentGrid).GetCornerPoint(0, 0)));
    }

    public override void PostTurnActions()
    {
        charging = chargePerTurn;
    }
}