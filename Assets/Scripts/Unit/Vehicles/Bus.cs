public class Bus : Vehicle
{
    protected override void SetUpSize()
    {
        size = 3;
        maxSpeed = 1;
    }

    public override void SetMovementPattern()
    {
        movementPattern.Add(new GridCoord(1, 0));
    }
}
