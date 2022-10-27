using UnityEngine;

public class Bloat : Enemy
{
    protected override void SetHealthAndDamage()
    {
        health = 1;
        damage = 1;
    }
    protected override void SetAdditionalTag()
    {
        unitTag = "Bloat";
    }

    public override void SetMovementPattern()
    {
        movementPattern.Add(new GridCoord(0, direction));
    }

    public override void TakeVehicleInTheWayAction()
    {
        skipTurn = 1;
        ExecuteConcussedMovement();
    }

    protected override void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Hit by a car! @({_currentGridPosition.x}, {_currentGridPosition.y})...");

        other.gameObject.GetComponentInParent<Unit>().DisableUnit(2);

        DestroySelf();
    }

    public override string GetName()
    {
        return "Bloat";
    }

    public override string GetDescription()
    {
        return "Movement Pattern: Moves 1 step forward per turn. <br> <br>" +
            "Vehicle in the way: Runs into vehicle and become stunned for 1 turn. <br> <br>" +
            "Additional effects: When a vehicle kills it, it will explode onto the vehicle, stunning the vehicle for 2 turns.";
    }
}