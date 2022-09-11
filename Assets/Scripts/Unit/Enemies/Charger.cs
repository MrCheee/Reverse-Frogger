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

    public override string GetName()
    {
        return "Charger";
    }

    public override string GetDescription()
    {
        return "Movement Pattern: Charges up its movement over 3 turns. It will charge forward for half the map in distance, " +
            "or until it hits a vehicle. <br> <br>" +
            "Vehicle in the way: Runs into vehicle and become stunned for 1 turn.";
    }
}