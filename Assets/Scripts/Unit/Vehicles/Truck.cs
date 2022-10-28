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