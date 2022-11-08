public class SpeedyCar : Vehicle
{
    protected override void SetUnitAttributes()
    {
        health = 4;
        damage = 1;
    }

    protected override void SetUpSize()
    {
        size = 1;
        maxSpeed = 4;
    }

    public override void SetMovementPattern()
    {
        movementPattern.Add(new GridCoord(1, 0));
        movementPattern.Add(new GridCoord(1, 0));
    }

    protected override void SetAdditionalTag()
    {
        unitTag = "Knockback-able Vehicle";
    }

    public override string GetName()
    {
        return "Fast Car";
    }

    public override string GetDescription()
    {
        return "Size: 1 grid <br> <br>" +
            "Speed: <br>-Base speed of 2 steps, Maximum speed of 4 steps. <br> <br>" +
            "Additional effects: <br>-Can be displaced by strong enemies. Manual lane change possible.";
    }
}