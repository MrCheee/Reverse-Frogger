public class Bus : Vehicle
{
    protected override void SetUnitAttributes()
    {
        health = 7;
        damage = 1;
    }

    protected override void SetUpSize()
    {
        size = 3;
        maxSpeed = 1;
    }

    public override void SetMovementPattern()
    {
        movementPattern.Add(new GridCoord(1, 0));
    }

    public override void DealDamageToPlayer()
    {
        gameStateManager.DamagePlayer(3);
    }

    public override string GetName()
    {
        return "Bus";
    }

    public override string GetDescription()
    {
        return "Size: 3 grids <br> <br>" +
            "Speed: <br>-Base speed of 1 step. Maximum speed of 1 step. <br> <br>" +
            "Additional effects: <br>-Cannot be displaced forcibly by an enemy. Manual lane change possible.";
    }
}
