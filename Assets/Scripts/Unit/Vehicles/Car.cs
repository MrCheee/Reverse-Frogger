using System.Linq;
using UnityEngine;

public class Car : Vehicle
{
    protected override void SetUnitAttributes()
    {
        Health = 3;
        Damage = 1;
        Size = 1;
        maxSpeed = 3;
        SpecialTag = "Knockback-able Vehicle";
    }

    protected override void SetMovementPattern()
    {
        movementPattern.Add(new GridCoord(moveDirection, 0));
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