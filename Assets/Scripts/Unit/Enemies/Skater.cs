using System.Collections;
using System.Linq;
using UnityEngine;

public class Skater : Enemy
{
    protected override void SetUnitAttributes()
    {
        Health = 1;
        Damage = 1;
        chargePerTurn = 0;
        SpecialTag = "Basic";
    }

    protected override void SetMovementPattern()
    {
        movementPattern.Add(new GridCoord(1, direction));
    }

    protected override void TakeVehicleInTheWayAction()
    {
        DisableUnit(1);
        ExecuteConcussedMovement();
    }

    protected override void ExecuteConcussedMovement()
    {
        int horizontalMovement = movementPattern.Last().x;
        GridCoord horizontalGrid = new GridCoord(_currentGridPosition.x + horizontalMovement, _currentGridPosition.y);

        // If there is a vehicle horizontally, remain in same grid concussed. Simulate corner move and back to middle
        if (FieldGrid.IsVehicleInTheWay(horizontalGrid))
        {
            commandStack.Enqueue(new MoveWithinGridCommand(FieldGrid.GetGrid(_currentGridPosition).GetCornerPoint(horizontalMovement, direction)));
            commandStack.Enqueue(new MoveWithinGridCommand(FieldGrid.GetGrid(_currentGridPosition).GetGridCentrePoint()));
            FieldGrid.AddGridToReposition(_currentGridPosition);
        }
        else  // If there is no vehicle horizontally, move to horizontal grid concussed. Simulate corner move and then to horizontal grid
        {
            commandStack.Enqueue(new MoveWithinGridCommand(FieldGrid.GetGrid(_currentGridPosition).GetCornerPoint(horizontalMovement, direction)));
            commandStack.Enqueue(new MoveToTargetGridCommand(horizontalGrid));
            FieldGrid.AddGridToReposition(horizontalGrid);
        }
    }

    protected override IEnumerator PostTurnActions()
    {
        if (actionTaken)
        {
            ReverseMotion();
            actionTaken = false;
        }
        animator.SetBool(movingAP, false);
        TurnInProgress = false;
        yield break;
    }

    private void ReverseMotion()
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