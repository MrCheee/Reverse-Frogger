using UnityEngine;

public class Soldier : Enemy
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

    public override string GetName()
    {
        return "Soldier";
    }

    public override string GetDescription()
    {
        return "Movement Pattern: Moves 1 step forward per turn. <br> <br>" +
            "Vehicle in the way: Runs into vehicle and become stunned for 1 turn.";
    }
}