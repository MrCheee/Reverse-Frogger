﻿public class Sprinter : Enemy
{
    protected override void SetUnitAttributes()
    {
        health = 1;
        damage = 1;
        chargePerTurn = 0;
    }

    public override void SetMovementPattern()
    {
        movementPattern.Add(new GridCoord(0, direction));
        movementPattern.Add(new GridCoord(0, direction));
    }

    public override void TakeVehicleInTheWayAction()
    {
        skipTurn = 1;
        ExecuteConcussedMovement();
    }

    public override string GetName()
    {
        return "Ghoul";
    }

    public override string GetDescription()
    {
        return "Movement Pattern: <br>-Moves 2 step forward per turn. <br> <br>" +
            "Vehicle in the way: <br>-Runs into the vehicle and becomes stunned for 1 turn.";
    }
}