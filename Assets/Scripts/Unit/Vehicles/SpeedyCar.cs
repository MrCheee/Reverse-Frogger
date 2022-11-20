public class SpeedyCar : Vehicle
{
    protected override void SetUnitAttributes()
    {
        Health = 3;
        Damage = 1;
        Size = 1;
        maxSpeed = 4;
        SpecialTag = "Knockback-able Vehicle";
    }

    protected override void SetMovementPattern()
    {
        movementPattern.Add(new GridCoord(moveDirection, 0));
        movementPattern.Add(new GridCoord(moveDirection, 0));
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