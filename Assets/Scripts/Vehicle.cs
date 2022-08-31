using System.Linq;

public abstract class Vehicle : Unit
{
    public int SpeedAddition { get; set; }

    public override void TakeTurn()
    {
        GridCoord projectedGridPos = currentGridPosition;
        foreach (GridCoord nextMove in movementPattern)
        {
            GridCoord nextGrid = Helper.AddGridCoords(projectedGridPos, nextMove);
            if (IsVehicleInTheWay(nextGrid) || IsBruteInTheWay(nextGrid))
            {
                break;
            }

            GiveMovementCommand(projectedGridPos, nextMove);
            projectedGridPos = nextGrid;
        }
    }

    public override void CheckConditionsToDestroy()
    {
        if (HasReachedEndOfRoad())
        {
            DestroySelf();
        }
    }

    public bool HasReachedEndOfRoad()
    {
        if (currentGridPosition.y < 4)
        {
            return currentGridPosition.x == 0;
        }
        else
        {
            return currentGridPosition.x == 10;
        }
    }

    public bool IsBruteInTheWay(GridCoord targetGrid)
    {
        return FieldGrid.IsWithinField(targetGrid) && FieldGrid.GetSingleGrid(targetGrid).GetUnitsTag().Contains("Brute");
    }
    public void reverseMotion()
    {
        movementPattern = movementPattern.Select(x => new GridCoord(x.x * -1, x.y)).ToList();
    }

}