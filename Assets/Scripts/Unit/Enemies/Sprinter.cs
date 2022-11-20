public class Sprinter : Enemy
{
    protected override void SetUnitAttributes()
    {
        Health = 1;
        Damage = 1;
        chargePerTurn = 0;
        SpecialTag = "Basic";
    }

    protected override void SetMovementPattern()
    {
        movementPattern.Add(new GridCoord(0, direction));
        movementPattern.Add(new GridCoord(0, direction));
    }

    protected override void TakeVehicleInTheWayAction()
    {
        DisableUnit(1);
        ExecuteConcussedMovement();
    }

    public override string GetName()
    {
        return "Ghoul";
    }

    public override string GetDescription()
    {
        return "Movement Pattern: <br>-Moves 2 step forward per turn. <br> <br>" +
            "Vehicle in the way: <br>-Runs into the vehicle and becomes stunned for 1 turn.";
    }
}