public class Truck : Vehicle
{
    protected override void SetUnitAttributes()
    {
        Health = 4;
        Damage = 1;
        Size = 2;
        maxSpeed = 2;
        SpecialTag = "Non-displacable Vehicle";
    }

    protected override void SetMovementPattern()
    {
        movementPattern.Add(new GridCoord(moveDirection, 0));
    }

    public override string GetName()
    {
        return "Truck";
    }

    public override string GetDescription()
    {
        return "Size: 2 grids <br> <br>" +
            "Speed: <br>-Base speed of 1 step, Maximum speed of 2 steps. <br> <br>" +
            "Additional effects: <br>-Cannot be displaced forcibly by an enemy. Manual lane change possible.";
    }
}