using UnityEngine;

public abstract class MoveCommand : Command
{
    protected Vector3 _moveDirection;
    protected Vector3 _target;
    protected bool moveReady = false;
    protected bool executed = false;

    protected void MoveToTarget(Unit unit)
    {
        unit.Move(calculateMoveDirection(_target, unit.transform.position));
        if (ReachedDestination(unit))
        {
            IsFinished = true;
        }
    }

    protected Vector3 calculateMoveDirection(Vector3 target_pos, Vector3 start_pos)
    {
        return (target_pos - start_pos).normalized;
    }

    protected bool ReachedDestination(Unit unit)
    {
        return Helper.vectorDistanceIgnoringYAxis(unit.gameObject.transform.position, _target) <= 0.1f;
    } 
}