using UnityEngine;

public class MoveToTargetGridCommand : MoveCommand
{
    private float direction = -1f;
    public GridCoord TargetGrid { get; set; }

    public MoveToTargetGridCommand(GridCoord targetGrid)
    {
        TargetGrid = targetGrid;
    }

    public override void Execute(Unit unit)
    {
        if (IsFinished) return;
        if (!moveReady)
        {
            Vector3 gridCentrePoint = FieldGrid.GetGrid(TargetGrid).GetGridCentrePoint();
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
        unit.UpdateGridMovement(TargetGrid);
    }
}