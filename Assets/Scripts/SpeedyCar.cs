public class SpeedyCar : Vehicle
{
    public override void SetMovementPattern()
    {
        movementPattern.Add(new GridCoord(1, 0));
        movementPattern.Add(new GridCoord(1, 0));
    }
}