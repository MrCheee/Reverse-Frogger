using System.Collections;

public class Charger : Enemy
{
    protected override void SetUnitAttributes()
    {
        Health = 1;
        Damage = 2;
        chargePerTurn = 3;
        Charging = chargePerTurn;
        SpecialTag = "Basic";
    }

    protected override void SetMovementPattern()
    {
        int totalLength = FieldGrid.NumOfLanes + 1;  // Dash half of the map, start to divider, then divider to end
        for (int i = 0; i < totalLength; i++)
        {
            movementPattern.Add(new GridCoord(0, direction));
        }
    }

    protected override void TakeVehicleInTheWayAction()
    {
        DisableUnit(1);
        ExecuteConcussedMovement();
    }

    public override string GetName()
    {
        return "Charger";
    }

    public override string GetDescription()
    {
        return "Movement Pattern: <br>-Charges up its movement over 3 turns. It will then charge forward 5 steps. <br> <br>" +
            "Vehicle in the way: <br>-Runs into the vehicle and becomes stunned for 1 turn.";
    }
}