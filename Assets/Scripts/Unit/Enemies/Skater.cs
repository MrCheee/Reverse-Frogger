﻿using System.Linq;

public class Skater : Enemy
{
    protected override void SetHealthAndDamage()
    {
        health = 1;
        damage = 1;
    }

    public override void SetMovementPattern()
    {
        movementPattern.Add(new GridCoord(1, direction));
    }

    public override void TakeVehicleInTheWayAction()
    {
        skipTurn = 1;
        ExecuteConcussedMovement();
    }

    protected override void ExecuteConcussedMovement()
    {
        GridCoord currentGrid = GetCurrentHeadGridPosition();
        GridCoord moveGrid = new GridCoord(movementPattern.Last().x, 0);
        GridCoord _targetGrid = new GridCoord(currentGrid.x + moveGrid.x, currentGrid.y + moveGrid.y);
        int right = moveGrid.x > 0 ? -1 : 1;
        commandStack.Enqueue(new MoveWithinGridCommand(FieldGrid.GetSingleGrid(_targetGrid).GetCornerPoint(right, direction)));
        commandStack.Enqueue(new MoveToGridCommand(moveGrid));
        FieldGrid.AddGridToReposition(Helper.AddGridCoords(currentGrid, moveGrid));
    }

    public override void PostTurnActions()
    {
        ReverseMotion();
    }

    public void ReverseMotion()
    {
        movementPattern = movementPattern.Select(x => new GridCoord(x.x * -1, x.y)).ToList();
    }

    public override string GetName()
    {
        return "Skater";
    }

    public override string GetDescription()
    {
        return "Movement Pattern: Moves 1 step forward left or forward right, alternating between them. <br> <br>" +
            "Vehicle in the way: Runs into vehicle and become stunned for 1 turn, while being displaced horizontally.";
    }
}