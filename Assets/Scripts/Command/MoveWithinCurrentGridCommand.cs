public class MoveWithinCurrentGridCommand : MoveCommand
{
    float manualYAdjustment;
    int _right;
    int _top;

    public MoveWithinCurrentGridCommand(int right, int top, float manualY = 0)
    {
        _right = right;
        _top = top;
        manualYAdjustment = manualY;
    }

    public override void Execute(Unit unit)
    {
        if (isFinished) return;
        if (!moveReady)
        {
            GridCoord currentGrid = unit.GetCurrentHeadGridPosition();
            _target = FieldGrid.GetSingleGrid(currentGrid).GetCornerPoint(_right, _top);
            _target.y = manualYAdjustment != 0 ? manualYAdjustment : unit.yAdjustment;
            _moveDirection = calculateMoveDirection(_target, unit.transform.position);
            FieldGrid.AddGridToReposition(currentGrid);
            moveReady = true;
        }
        MoveToTarget(unit);
    }
}