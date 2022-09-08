using UnityEngine;

public class Bloat : Enemy
{
    protected override void SetHealthAndDamage()
    {
        health = 1;
        damage = 1;
    }

    public override void SetMovementPattern()
    {
        movementPattern.Add(new GridCoord(0, 1));
    }

    public override void TakeVehicleInTheWayAction()
    {
        skipTurn = 1;

        GridCoord currentGrid = GetCurrentGridPosition();
        commandStack.Enqueue(new MoveWithinGridCommand(FieldGrid.GetSingleGrid(currentGrid).GetCornerPoint(0, 1)));
        commandStack.Enqueue(new MoveWithinGridCommand(FieldGrid.GetSingleGrid(currentGrid).GetCornerPoint(0, 0)));
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<Unit>().TurnInProgress)
        {
            Debug.Log($"Hit by a car! @({_currentGridPosition.x}, {_currentGridPosition.y})...");

            other.gameObject.GetComponent<Unit>().DisableUnit(1);

            DestroySelf();
        }

    }
}