using UnityEngine;

public abstract class MoveCommand : Command
{
    protected Vector3 _moveDirection;
    protected Vector3 _target;
    protected bool moveReady = false;
    protected bool executed = false;

    public void MoveToTarget(Unit unit)
    {
        unit.Move(calculateMoveDirection(_target, unit.transform.position));
        //unit.Move(_moveDirection);
        if (unit.ReachedPosition(_target))
        {
            IsFinished = true;
        }
    }

    public Vector3 calculateMoveDirection(Vector3 target_pos, Vector3 start_pos)
    {
        return (target_pos - start_pos).normalized;
    }
}