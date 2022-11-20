public class Bus : Vehicle
{
    protected override void SetUnitAttributes()
    {
        Health = 5;
        Damage = 1;
        Size = 3;
        maxSpeed = 1;
        SpecialTag = "Non-displacable Vehicle";
    }

    protected override void SetMovementPattern()
    {
        movementPattern.Add(new GridCoord(moveDirection, 0));
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
