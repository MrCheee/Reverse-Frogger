public class Sprinter : Enemy
{
    protected override void SetHealthAndDamage()
    {
        health = 1;
        damage = 1;
    }

    public override void SetMovementPattern()
    {
        movementPattern.Add(new GridCoord(0, direction));
        movementPattern.Add(new GridCoord(0, direction));
    }

    public override void TakeVehicleInTheWayAction()
    {
        skipTurn = 2;
        ExecuteConcussedMovement();
    }

    public override string GetName()
    {
        return "Sprinter";
    }

    public override string GetDescription()
    {
        return "Movement Pattern: Moves 2 step forward per turn. <br> <br>" +
            "Vehicle in the way: Runs into vehicle and become stunned for 2 turns.";
    }
}