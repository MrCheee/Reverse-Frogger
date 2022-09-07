using UnityEngine;
using System.Linq;

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
    protected bool moveReady = false;

    public void MoveToTarget(Unit unit)
    {
        unit.Move(_moveDirection);
        if (unit.ReachedPosition(_target))
        {
            //Debug.Log("Completed Movement");
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
            GridCoord currentGrid = unit.GetCurrentGridPosition();
            _targetGrid = new GridCoord(currentGrid.x + _toMove.x, currentGrid.y + _toMove.y);
            _target = FieldGrid.GetSingleGrid(_targetGrid).GetGridCentrePoint();
            _moveDirection = calculateMoveDirection(_target, unit.transform.position);

            UpdateGridOnMovement(unit);
            moveReady = true;
        }
        MoveToTarget(unit);
    }

    public void UpdateGridOnMovement(Unit unit)
    {
        unit.RemoveFromFieldGridPosition();
        unit.AddToFieldGridPosition(_targetGrid);
    }
}

public class MoveToGridInbetweenCommand : MoveCommand
{
    private GridCoord _toMove;
    private GridCoord _targetGrid;
    private int[] toBottomInBetweenLane = new int[2];
    private int dividerY;

    public MoveToGridInbetweenCommand(GridCoord moveGrid)
    {
        dividerY = FieldGrid.GetDividerLaneNum();
        toBottomInBetweenLane[0] = dividerY - 1;
        toBottomInBetweenLane[1] = dividerY + FieldGrid.GetNumberOfLanes();
        _toMove = moveGrid;
    }

    public override void Execute(Unit unit)
    {
        Debug.Log("Executing Move To Grid Inbetween Command");
        GridCoord currentGrid = unit.GetCurrentGridPosition();
        _targetGrid = new GridCoord(currentGrid.x + _toMove.x, currentGrid.y + _toMove.y);

        UpdateGridOnMovement(unit);

        int posY = currentGrid.y;
        int left = posY < dividerY ? 1 : -1;
        int top = toBottomInBetweenLane.Contains(posY) ? -1 : 1;
        int front = 0;
        //int front = toBottomInBetweenLane.Contains(posY) ? 1 : -1;

        unit.IssueCommand(new MoveWithinGridCommand(FieldGrid.GetSingleGrid(_targetGrid).GetCornerPoint(left, top)));
        unit.IssueCommand(new MoveWithinGridCommand(FieldGrid.GetSingleGrid(_targetGrid).GetInBetweenPoint(front, top)));

        isFinished = true;
    }

    public void UpdateGridOnMovement(Unit unit)
    {
        unit.RemoveFromFieldGridPosition();
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
        if (!moveReady)
        {
            _moveDirection = calculateMoveDirection(_target, unit.transform.position);
            moveReady = true;
        }
        MoveToTarget(unit);
    }
}