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

    public override string GetName()
    {
        return "Bus";
    }

    public override string GetDescription()
    {
        return "Size: 3 width <br> <br>" +
            "Speed: Base 1, Max 1 steps <br> <br>" +
            "Additional effects: Cannot be displaced forcibly by an enemy. Manual Lane Change possible.";
    }
}
