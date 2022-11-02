using UnityEngine;
// Any movement from one grid to another will be done by giving the Grid from, and Grid to, and using centrepoints of those grid
// for the target
public class MoveToGridCommand : MoveCommand
{
    private GridCoord _toMove;
    private GridCoord _targetGrid;
    
    public MoveToGridCommand(GridCoord moveGrid)
    {
        _toMove = moveGrid;
    }

    public override void Execute(Unit unit)
    {
        if (isFinished) return;
        if (!moveReady)
        {
            GridCoord currentGrid = unit.GetCurrentHeadGridPosition();
            _targetGrid = new GridCoord(currentGrid.x + _toMove.x, currentGrid.y + _toMove.y);
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