using UnityEngine;

public abstract class Command
{
    public abstract void Execute(Unit unit);
    protected bool isFinished;
    public bool IsFinished { get => isFinished; protected set => isFinished = value; }
}

public abstract class MoveCommand : Command
{
    protected Vector3 _moveDirection;
    protected Vector3 _target;
    protected bool hasMoved = false;

    public void MoveToTarget(Unit unit)
    {
        unit.Move(_moveDirection);
        if (unit.ReachedPosition(_target))
        {
            Debug.Log("Completed Movement");
            IsFinished = true;
        }
    }

    public Vector3 calculateMoveDirection(Vector3 target_pos, Vector3 start_pos)
    {
        return (target_pos - start_pos).normalized;
    }
}

// Any movement from one grid to another will be done by giving the Grid from, and Grid to, and using centrepoints of those grid
// for the target
public class MoveToGridCommand : MoveCommand
{
    private GridCoord _currentGrid;
    private GridCoord _targetGrid;
    
    public MoveToGridCommand(GridCoord currentGrid, GridCoord moveGrid)
    {
        _currentGrid = currentGrid;
        _targetGrid = new GridCoord(currentGrid.x + moveGrid.x, currentGrid.y + moveGrid.y);
        _target = FieldGrid.GetSingleGrid(_targetGrid).GetGridCentrePoint();
    }

    public override void Execute(Unit unit)
    {
        if (isFinished) return;
        if (!hasMoved)
        {
            _moveDirection = calculateMoveDirection(_target, unit.transform.position);
            UpdateGridOnMovement(unit);
            hasMoved = true;
        }
        MoveToTarget(unit);
    }

    public void UpdateGridOnMovement(Unit unit)
    {
        unit.RemoveFromFieldGridPosition(_currentGrid);
        unit.AddToFieldGridPosition(_targetGrid);
    }
}

// Any movement and positioning within the grid will be controlled by the Grid classes. They will assign Vector3 movement.
public class MoveWithinGridCommand : MoveCommand
{
    public MoveWithinGridCommand(Vector3 target)
    {
        _target = target;
    }

    public override void Execute(Unit unit)
    {
        if (isFinished) return;
        if (!hasMoved)
        {
            _moveDirection = calculateMoveDirection(_target, unit.transform.position);
            hasMoved = true;
        }
        MoveToTarget(unit);
    }
}

public class DestroyCommand : Command
{
    public override void Execute(Unit unit)
    {
        unit.DestroySelf();
    }
}