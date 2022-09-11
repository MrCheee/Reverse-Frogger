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
        return "Size: 2 width <br> <br>" +
            "Speed: Base 1, Max 2 steps <br> <br>" +
            "Additional effects: Cannot be displaced forcibly by an enemy. Manual Lane Change possible.";
    }
}