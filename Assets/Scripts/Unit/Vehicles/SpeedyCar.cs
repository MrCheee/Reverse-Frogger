public class SpeedyCar : Vehicle
{
    protected override void SetUpSize()
    {
        size = 1;
        maxSpeed = 5;
    }

    public override void SetMovementPattern()
    {
        movementPattern.Add(new GridCoord(1, 0));
        movementPattern.Add(new GridCoord(1, 0));
    }
}