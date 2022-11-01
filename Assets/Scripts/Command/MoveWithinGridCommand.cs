using UnityEngine;
// Any movement and positioning within the grid will be controlled by the Grid classes. They will assign Vector3 movement.
public class MoveWithinGridCommand : MoveCommand
{
    float manualYAdjustment;

    public MoveWithinGridCommand(Vector3 target, float manualY = 0)
    {
        manualYAdjustment = manualY;
        _target = target;
    }

    public override void Execute(Unit unit)
    {
        if (isFinished) return;
        if (!moveReady)
        {
            _target.y = manualYAdjustment != 0 ? manualYAdjustment : unit.yAdjustment;
            _moveDirection = calculateMoveDirection(_target, unit.transform.position);
            moveReady = true;
        }
        MoveToTarget(unit);
    }
}