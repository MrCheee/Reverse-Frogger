using System.Linq;

public class Skater : Enemy
{
    public override void SetMovementPattern()
    {
        movementPattern.Add(new GridCoord(1, 1));
    }

    public void reverseMotion()
    {
        movementPattern = movementPattern.Select(x => new GridCoord(x.x * -1, x.y)).ToList();
    }

    public override void TakeTurn()
    {
        if (HasCrossedTheRoad()) { return; }

        GridCoord projectedGridPos = currentGridPosition;
        foreach (GridCoord nextMove in movementPattern)
        {
            GridCoord nextGrid = Helper.AddGridCoords(projectedGridPos, nextMove);
            if (IsVehicleInTheWay(nextGrid))
            {
                break;
            }

            GiveMovementCommand(projectedGridPos, nextMove);
            projectedGridPos = nextGrid;
            reverseMotion();
        }
    }
}