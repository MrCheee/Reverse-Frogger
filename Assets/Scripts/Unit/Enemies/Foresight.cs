using System.Collections;
using System.Linq;
using UnityEngine;

public class Foresight : Enemy
{
    protected override void SetUnitAttributes()
    {
        Health = 1;
        Damage = 1;
        chargePerTurn = 0;
        SpecialTag = "Foresight";
    }

    protected override void SetMovementPattern()
    {
        movementPattern.Add(new GridCoord(-1, direction));
        movementPattern.Add(new GridCoord(0, direction));
        movementPattern.Add(new GridCoord(1, direction));
    }

    protected override IEnumerator TakeTurn()
    {
        if (HasCrossed)
        {
            TurnInProgress = false;
            yield break;
        }
        if (ToSkipTurn()) yield break;

        actionTaken = true;

        bool[] vehiclesInTheWay = new bool[3];
        for (int i = 0; i < movementPattern.Count; i++)
        {
            vehiclesInTheWay[i] = FieldGrid.IsVehicleInTheWay(Helper.AddGridCoords(_currentGridPosition, movementPattern[i]));
        }

        // Quick Exit if all no possible forward motion
        if (vehiclesInTheWay.All(x => x))
        {
            TurnInProgress = false;
            yield break;
        }

        bool isInLeftLane = _currentGridPosition.y <= FieldGrid.DividerY;
        int maxLeft = FieldGrid.FieldBuffer;
        int maxRight = FieldGrid.FieldLength - FieldGrid.FieldBuffer - 1;
        GridCoord nextMove = new GridCoord(0, 0);

        if (isInLeftLane)
        {
            // If all clear, take leftmost, or forward
            if (vehiclesInTheWay.All(x => !x))
            {
                if (_currentGridPosition.x == maxLeft) // Cannot move left anymore
                {
                    nextMove = movementPattern[1]; // Move straight
                }
                else
                {
                    nextMove = movementPattern[0]; // Move left
                }
            } 
            else if (!vehiclesInTheWay[1] && !vehiclesInTheWay[2])  // If no vehicle in front, and no vehicle on right
            {
                nextMove = movementPattern[1]; // Move straight
            } 
            else if (!vehiclesInTheWay[2])  // If no vehicle on the right
            {
                if (_currentGridPosition.x != maxRight) // Check if still can move right
                {
                    nextMove = movementPattern[2]; // Move right
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
                    nextMove = movementPattern[1]; // Move straight
                }
                else
                {
                    nextMove = movementPattern[2]; // Move right
                }
            }
            else if (!vehiclesInTheWay[1] && !vehiclesInTheWay[0])  // If no vehicle in front, and no vehicle on left
            {
                nextMove = movementPattern[1]; // Move straight
            }
            else if (!vehiclesInTheWay[0])  // If no vehicle on the left
            {
                if (_currentGridPosition.x != maxLeft) // Check if still can move left
                {
                    nextMove = movementPattern[0]; // Move left
                }
            }
        }

        if (nextMove.y == direction)
        {
            animator.SetBool(movingAP, true);
            if (nextMove.y == -1)
            {
                GetComponentInChildren<SpriteRenderer>().flipX = true;
            }
            else
            {
                GetComponentInChildren<SpriteRenderer>().flipX = false;
            }
        }

        GridCoord nextGrid = Helper.AddGridCoords(_currentGridPosition, nextMove);
        IssueCommand(new MoveToTargetGridCommand(nextGrid));

        // If it has moved forward
        if (nextMove.y == direction)
        {
            // Check if crossed the road
            if (HasCrossedTheRoad(nextGrid))
            {
                TriggerDamageOnPlayer();
                MarkedAsCrossed();
            }
            // Check if ended up on the divider, then reposition to the centre of divider on the same turn
            else if (nextGrid.y == FieldGrid.DividerY)
            {
                int gridCentreX = (FieldGrid.FieldLength - 1) / 2;
                GridCoord centreGrid = new GridCoord(gridCentreX, FieldGrid.DividerY);
                if (gridCentreX - nextGrid.x < 0)
                {
                    GetComponentInChildren<SpriteRenderer>().flipX = true;
                } else
                {
                    GetComponentInChildren<SpriteRenderer>().flipX = false;
                }
                IssueCommand(new MoveToTargetGridCommand(centreGrid));
            }
        }
        TurnInProgress = false;
    }

    public override string GetName()
    {
        return "Mutated Brain";
    }

    public override string GetDescription()
    {
        return "Movement Pattern: <br>-Moves 1 step per turn, either forward, diagonal left or diagonal right. <br> <br>" +
            "Vehicle in the way: <br>-It will not move. <br> <br>" +
            "Additional Effects: <br>-It determines its movement based on vehicle spots in front of it and will try to not intentionally move into a vehicle's path within its line of sight." +
            "<br>-Always repositon back to the middle when it reaches the divider.";
    }
}