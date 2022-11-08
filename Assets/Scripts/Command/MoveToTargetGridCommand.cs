using UnityEngine;

public class MoveToTargetGridCommand : MoveCommand
{
    private float direction = -1f;
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
            if (unit.yAdjustment > 0)
            {
                gridCentrePoint.z -= direction * 1.25f;
            }
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