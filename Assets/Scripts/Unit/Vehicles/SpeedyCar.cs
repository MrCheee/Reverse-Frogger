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
        tag = "Knockback-able Vehicle";
    }
}