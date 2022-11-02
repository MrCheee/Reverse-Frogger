using UnityEngine;

public class MoveToTargetGridCommand : MoveCommand
{
    private GridCoord _targetGrid;

    public MoveToTargetGridCommand(GridCoord targetGrid)
    {
        _targetGrid = targetGrid;
    }

    public override void Execute(Unit unit)
    {
        if (isFinished) return;
        if (!moveReady)
        {
            Vector3 gridCentrePoint = FieldGrid.GetSingleGrid(_targetGrid).GetGridCentrePoint();
            gridCentrePoint.y = unit.yAdjustment;
            _target = gridCentrePoint;
            _moveDirection = calculateMoveDirection(_target, unit.transform.position);

            UpdateGridOnMovement(unit);
            moveReady = true;
        }
        if (!executed)
        {
            if (unit.GetName() != "Shield" && _moveDirection.x != 0)
            {
                bool toFlip = _moveDirection.x < 0;
                unit.FlipUnitSprite(toFlip);
            }
            executed = true;
        }
        MoveToTarget(unit);
    }

    public void UpdateGridOnMovement(Unit unit)
    {
        unit.RemoveFromFieldGridPosition();
        unit.AddToFieldGridPosition(_targetGrid);
    }
}