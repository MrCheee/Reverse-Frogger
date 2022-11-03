using System.Collections;
using System.Linq;
using UnityEngine;

public class BabyForesight : Enemy
{
    protected override void SetHealthAndDamage()
    {
        health = 1;
        damage = 1;
    }

    public override void SetMovementPattern()
    {
        movementPattern.Add(new GridCoord(-1, direction));
        movementPattern.Add(new GridCoord(0, direction));
        movementPattern.Add(new GridCoord(1, direction));
    }

    public override IEnumerator TakeTurn()
    {
        if (Crossed)
        {
            TurnInProgress = false;
            yield break;
        }
        if (ToSkipTurn()) yield break;

        actionTaken = true;

        bool[] vehiclesInTheWay = new bool[3];
        for (int i = 0; i < movementPattern.Count; i++)
        {
            vehiclesInTheWay[i] = Helper.IsVehicleInTheWay(Helper.AddGridCoords(_currentGridPosition, movementPattern[i]));
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
            animator.SetTrigger("ToMove");
            animator.SetBool("Moving", true);
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
        GiveMovementCommand(nextMove);

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
            else if (nextGrid.y == FieldGrid.GetDividerLaneNum())
            {
                int gridCentre = (FieldGrid.GetMaxLength() - 1) / 2;
                GridCoord repositionToCentreMove = new GridCoord(gridCentre - nextGrid.x, 0);
                if (repositionToCentreMove.y == -1)
                {
                    GetComponentInChildren<SpriteRenderer>().flipX = true;
                } else
                {
                    GetComponentInChildren<SpriteRenderer>().flipX = false;
                }
                GiveMovementCommand(repositionToCentreMove);
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
        return "Movement Pattern: <br>-Moves 1 step per turn, either diagonal left or straight forward or diagonal right. <br> <br>" +
            "Vehicle in the way: <br>-It will not move. <br> <br>" +
            "Additional Effects: <br>-It determines its movement based on vehicle spots in front of it and will try to not intentionally move into a vehicle's path within its line of sight." +
            "<br>-Always repositon back to the middle when it reaches the divider.";
    }
}