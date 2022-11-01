using UnityEngine;
using System.Linq;

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
        GridCoord currentGrid = unit.GetCurrentHeadGridPosition();
        _targetGrid = new GridCoord(currentGrid.x + _toMove.x, currentGrid.y + _toMove.y);

        UpdateGridOnMovement(unit);

        int posY = currentGrid.y;
        int right = posY < dividerY ? 1 : -1;
        int top = toBottomInBetweenLane.Contains(posY) ? -1 : 1;
        int front = 0;

        unit.IssueCommand(new MoveWithinGridCommand(FieldGrid.GetSingleGrid(_targetGrid).GetCornerPoint(right, top)));
        unit.IssueCommand(new MoveWithinGridCommand(FieldGrid.GetSingleGrid(_targetGrid).GetInBetweenPoint(front, top)));

        isFinished = true;
    }

    public void UpdateGridOnMovement(Unit unit)
    {
        unit.RemoveFromFieldGridPosition();
        unit.AddToFieldGridPosition(_targetGrid);
    }
}
