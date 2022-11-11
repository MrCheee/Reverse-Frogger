﻿using UnityEngine;

public class Soldier : Enemy
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
    }

    public override void TakeVehicleInTheWayAction()
    {
        DisableUnit(1);
        ExecuteConcussedMovement();
    }

    public override string GetName()
    {
        return "Blob";
    }

    public override string GetDescription()
    {
        return "Movement Pattern: <br>-Moves 1 step forward per turn. <br> <br>" +
            "Vehicle in the way: <br>-Runs into the vehicle and becomes stunned for 1 turn.";
    }
}