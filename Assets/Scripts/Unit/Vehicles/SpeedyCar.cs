public class SpeedyCar : Vehicle
{
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
        return "Size: 1 width <br> <br>" +
            "Speed: Base 2, Max 4 steps <br> <br>" +
            "Additional effects: Can be displaced by strong enemies. Manual Lane Change possible.";
    }
}