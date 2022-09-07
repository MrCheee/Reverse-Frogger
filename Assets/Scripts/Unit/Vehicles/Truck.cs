public class Truck : Vehicle
{
    protected override void SetUpSize()
    {
        size = 2;
        maxSpeed = 2;
    }

    public override void SetMovementPattern()
    {
        movementPattern.Add(new GridCoord(1, 0));
    }
}