using System.Collections;
using System.Linq;

public class BabyForesight : Enemy
{
    protected override void SetHealthAndDamage()
    {
        health = 1;
        damage = 1;
    }

    public override void SetMovementPattern()
    {
        movementPattern.Add(new GridCoord(-1, 1));
        movementPattern.Add(new GridCoord(0, 1));
        movementPattern.Add(new GridCoord(1, 1));
    }

    public override IEnumerator TakeTurn()
    {
        if (ToSkipTurn()) yield break;
        if (HasCrossedTheRoad()) yield break;

        TurnInProgress = true;

        bool[] vehiclesInTheWay = new bool[3];
        for (int i = 0; i < movementPattern.Count; i++)
        {
            GridCoord nextGrid = Helper.AddGridCoords(_currentGridPosition, movementPattern[i]);
            vehiclesInTheWay[i] = Helper.IsVehicleInTheWay(nextGrid);
        }

        // Quick Exit if all no possible forward motion
        if (vehiclesInTheWay.All(x => x))
        {
            TurnInProgress = false;
            yield break;
        }

        bool isInLeftLane = _currentGridPosition.y < FieldGrid.GetDividerLaneNum();

        int maxLeft = FieldGrid.GetFieldBuffer();
        int maxRight = FieldGrid.GetMaxLength() - FieldGrid.GetFieldBuffer() - 1;

        if (isInLeftLane)
        {
            // If all clear, take leftmost, or forward
            if (vehiclesInTheWay.All(x => !x))
            {
                if (_currentGridPosition.x == maxLeft) // Cannot move left anymore
                {
                    GiveMovementCommand(movementPattern[1]); // Move straight
                }
                else
                {
                    GiveMovementCommand(movementPattern[0]); // Move left
                }
            } 
            else if (!vehiclesInTheWay[1] && !vehiclesInTheWay[2])  // If no vehicle in front, and no vehicle on right
            {
                GiveMovementCommand(movementPattern[1]); // Move straight
            } 
            else if (!vehiclesInTheWay[2])  // If no vehicle on the right
            {
                if (_currentGridPosition.x != maxRight) // Check if still can move right
                {
                    GiveMovementCommand(movementPattern[2]); // Move right
                }
            }
        }
        else
        {
            // If all clear, take rightmost, or forward
            if (vehiclesInTheWay.All(x => !x))
            {
                if (_currentGridPosition.x == maxRight) // Cannot move right anymore
                {
                    GiveMovementCommand(movementPattern[1]); // Move straight
                }
                else
                {
                    GiveMovementCommand(movementPattern[2]); // Move right
                }
            }
            else if (!vehiclesInTheWay[1] && !vehiclesInTheWay[0])  // If no vehicle in front, and no vehicle on left
            {
                GiveMovementCommand(movementPattern[1]); // Move straight
            }
            else if (!vehiclesInTheWay[0])  // If no vehicle on the left
            {
                if (_currentGridPosition.x != maxLeft) // Check if still can move left
                {
                    GiveMovementCommand(movementPattern[0]); // Move left
                }
            }
        }
        TurnInProgress = false;
        PostTurnActions();
    }
}