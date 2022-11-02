using UnityEngine;

public class Bloat : Enemy
{
    protected override void SetHealthAndDamage()
    {
        health = 1;
        damage = 2;
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
        animator.SetTrigger("Killed");
        string killedInfo = $"{gameObject.GetComponent<Unit>().GetName()} has been run over at Grid [{_currentGridPosition.x}, {_currentGridPosition.y}]!";
        gameStateManager.EnemyKilled(transform.position, killedInfo);
        other.gameObject.GetComponentInParent<Unit>().DisableUnit(2);
        DestroySelf(1f);
    }

    public override string GetName()
    {
        return "Bloat";
    }

    public override string GetDescription()
    {
        return "Movement Pattern: <br>-Moves 1 step forward per turn. <br> <br>" +
            "Vehicle in the way: <br>-Runs into the vehicle and becomes stunned for 1 turn. <br> <br>" +
            "Additional effects: <br>-When a vehicle kills it, it will explode onto the vehicle, stunning the vehicle for 2 turns.";
    }
}