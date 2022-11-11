using System.Linq;

public class Car : Vehicle
{
    protected override void SetUnitAttributes()
    {
        health = 3;
        damage = 1;
    }

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
        return "Size: 1 grid <br> <br>" +
            "Speed: <br>-Base speed of 1 step, Maximum speed of 3 steps. <br> <br>" +
            "Additional effects: <br>-Can be displaced by strong enemies. Manual lane change possible.";
    }
}