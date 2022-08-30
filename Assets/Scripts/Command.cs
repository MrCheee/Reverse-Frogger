using UnityEngine;

public abstract class Command
{
    public abstract void Execute(Unit unit);
    private bool isFinished;
    public bool IsFinished { get => isFinished; protected set => isFinished = value; }
}

public class MoveCommand : Command
{
    private Vector3 _moveDirection; 
    private Vector3 _target;

    public MoveCommand(Vector3 moveDirection, Vector3 target)
    {
        _target = target;
        _moveDirection = moveDirection;
    }

    public MoveCommand(GridCoord currentGrid, GridCoord moveGrid)
    {
        Vector3 _current = FieldGrid.GetSingleGrid(currentGrid.x, currentGrid.y).GetGridPoint();
        _target = FieldGrid.GetSingleGrid(currentGrid.x + moveGrid.x, currentGrid.y + moveGrid.y).GetGridPoint();
        _moveDirection = (_target - _current).normalized;
    }

    public override void Execute(Unit unit)
    {
        unit.Move(_moveDirection);
        if (unit.ReachedPosition(_target))
        {
            IsFinished = true;
        }
    }
}