using System.Collections;
using System.Linq;
using UnityEngine;

public class Skater : Enemy
{
    protected override void SetUnitAttributes()
    {
        health = 1;
        damage = 1;
        chargePerTurn = 0;
    }

    public override void SetMovementPattern()
    {
        movementPattern.Add(new GridCoord(1, direction));
    }

    public override void TakeVehicleInTheWayAction()
    {
        DisableUnit(1);
        ExecuteConcussedMovement();
    }

    protected override void ExecuteConcussedMovement()
    {
        GridCoord currentGrid = GetCurrentHeadGridPosition();
        GridCoord moveGrid = new GridCoord(movementPattern.Last().x, 0);
        GridCoord _targetGrid = new GridCoord(currentGrid.x + moveGrid.x, currentGrid.y + moveGrid.y);

        // If there is a vehicle horizontally, remain in same grid concussed. Simulate corner move and back to middle
        if (Helper.IsVehicleInTheWay(_targetGrid))
        {
            commandStack.Enqueue(new MoveWithinGridCommand(FieldGrid.GetSingleGrid(currentGrid).GetCornerPoint(moveGrid.x, direction)));
            commandStack.Enqueue(new MoveWithinGridCommand(FieldGrid.GetSingleGrid(currentGrid).GetGridCentrePoint()));
            FieldGrid.AddGridToReposition(currentGrid);
        }
        else  // If there is no vehicle horizontally, move to horizontal grid concussed. Simulate corner move and then to horizontal grid
        {
            commandStack.Enqueue(new MoveWithinGridCommand(FieldGrid.GetSingleGrid(currentGrid).GetCornerPoint(moveGrid.x, direction)));
            commandStack.Enqueue(new MoveToGridCommand(moveGrid));
            FieldGrid.AddGridToReposition(Helper.AddGridCoords(currentGrid, moveGrid));
        }
    }

    public override IEnumerator PostTurnActions()
    {
        if (actionTaken)
        {
            ReverseMotion();
            actionTaken = false;
        }
        animator.SetBool("Moving", false);
        TurnInProgress = false;
        yield break;
    }

    public void ReverseMotion()
    {
        movementPattern = movementPattern.Select(x => new GridCoord(x.x * -1, x.y)).ToList();
    }

    public override string GetName()
    {
        return "Killer Crab";
    }

    public override string GetDescription()
    {
        return "Movement Pattern: <br>-Moves 1 step diagonally forward (left or right), alternating between left and right. <br> <br>" +
            "Vehicle in the way: <br>-Runs into the vehicle, becomes stunned for 1 turn, and is displaced horizontally. If there is a vehicle " +
            "horizontally as well, then it won't be displaced.";
    }
}