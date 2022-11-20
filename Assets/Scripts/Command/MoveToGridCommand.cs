using UnityEngine;
// Any movement from one grid to another will be done by giving the Grid from, and Grid to, and using centrepoints of those grid
// for the target
public class MoveToGridCommand : MoveCommand
{
    private float direction = -1f;
    private GridCoord _toMove;
    private GridCoord _targetGrid;
    
    public MoveToGridCommand(GridCoord moveGrid)
    {
        _toMove = moveGrid;
    }

    public override void Execute(Unit unit)
    {
        if (IsFinished) return;
        if (!moveReady)
        {
            GridCoord currentGrid = unit.GetCurrentHeadGridPosition();
            _targetGrid = new GridCoord(currentGrid.x + _toMove.x, currentGrid.y + _toMove.y);
            Vector3 gridCentrePoint = FieldGrid.GetGrid(_targetGrid).GetGridCentrePoint();
            
            gridCentrePoint.y = unit.VerticalDisplacement;
            if (unit.VerticalDisplacement > 0)
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
            if (unit is Enemy && unit.GetName() != "Shield" && _moveDirection.x != 0)
            {
                bool toFlip = _moveDirection.x < 0;
                unit.GetComponentInChildren<SpriteRenderer>().flipX = toFlip;
            }
            executed = true;
        }
        MoveToTarget(unit);
    }

    public void UpdateGridOnMovement(Unit unit)
    {
        unit.UpdateGridMovement(_targetGrid);
    }
}