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
            movementPattern.Add(new GridCoord(0, direction));
        }
    }

    public override void TakeVehicleInTheWayAction()
    {
        skipTurn = 1;
        ExecuteConcussedMovement();
    }

    public override string GetName()
    {
        return "Charger";
    }

    public override string GetDescription()
    {
        return "Movement Pattern: <br>-Charges up its movement over 3 turns. It will then charge forward 5 steps. <br> <br>" +
            "Vehicle in the way: <br>-Runs into the vehicle and becomes stunned for 1 turn.";
    }
}