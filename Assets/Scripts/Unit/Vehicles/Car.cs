using System.Linq;

public class Car : Vehicle
{
    protected override void SetUpSize()
    {
        size = 1;
        maxSpeed = 3;
    }

    public override void SetMovementPattern()
    {
        movementPattern.Add(new GridCoord(1, 0));
    }

    protected override void SetAdditionalTag()
    {
        unitTag = "Knockback-able Vehicle";
    }

    public override string GetName()
    {
        return "Car";
    }

    public override string GetDescription()
    {
        return "Size: 1 width <br> <br>" +
            "Speed: Base 1, Max 3 steps <br> <br>" +
            "Additional effects: Can be displaced by strong enemies. Manual Lane Change possible.";
    }
}