public class MoveWithinCurrentGridCommand : MoveCommand
{
    float manualYAdjustment;
    float _right;
    float _top;

    public MoveWithinCurrentGridCommand(float right, float top, float manualY = 0)
    {
        _right = right;
        _top = top;
        manualYAdjustment = manualY;
    }

    public override void Execute(Unit unit)
    {
        if (IsFinished) return;
        if (!moveReady)
        {
            GridCoord currentGrid = unit.GetCurrentHeadGridPosition();
            _target = FieldGrid.GetGrid(currentGrid).GetCornerPoint(_right, _top);
            _target.y = manualYAdjustment != 0 ? manualYAdjustment : unit.VerticalDisplacement;
            _moveDirection = calculateMoveDirection(_target, unit.transform.position);
            FieldGrid.AddGridToReposition(currentGrid);
            moveReady = true;
        }
        MoveToTarget(unit);
    }
}