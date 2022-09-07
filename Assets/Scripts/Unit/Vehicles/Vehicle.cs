using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Vehicle : Unit
{
    public int SpeedAddition { get; set; }

    protected int size = 1;
    protected int maxSpeed = 1;

    private int laneSpeedAddition = 0;
    protected int dividerY = FieldGrid.GetFieldBuffer() + 1 + FieldGrid.GetNumberOfLanes();
    protected GridCoord[] _currentGridPosition;

    protected override void Awake()
    {
        SetUpSize();
        _currentGridPosition = new GridCoord[size];
        base.Awake();
    }

    protected abstract void SetUpSize();
    
    public override GridCoord GetCurrentGridPosition()
    {
        return _currentGridPosition[0];
    }

    public override void AddToFieldGridPosition(GridCoord position)
    {
        SetCurrentGridPosition(position);
        for (int i = 0; i < _currentGridPosition.Length; i++)
        {
            FieldGrid.GetSingleGrid(_currentGridPosition[i]).AddObject(gameObject);
        }
    }

    public override void RemoveFromFieldGridPosition()
    {
        for (int i = 0; i < _currentGridPosition.Length; i++)
        {
            FieldGrid.GetSingleGrid(_currentGridPosition[i]).RemoveObject(gameObject.GetInstanceID());
        }
    }

    public override void SetCurrentGridPosition(GridCoord position)
    {
        for (int i = 0; i < size; i++)
        {
            _currentGridPosition[i] = position;
            position = GetNextBodyPosition(position);
        }
    }

    public override void PreTurnActions()
    {
        return;
    }
    public override void PostTurnActions()
    {
        return;
    }

    public override IEnumerator TakeTurn()
    {
        TurnInProgress = true;
        PreTurnActions();
        int retries = 0;
        Queue<GridCoord> moveQueue = new Queue<GridCoord>(movementPattern);
        moveQueue = UpdateMovementWithLaneSpeed(moveQueue);
        GridCoord nextMove = moveQueue.Peek();
        GridCoord nextGrid = Helper.AddGridCoords(_currentGridPosition[0], nextMove);
        while (moveQueue.Count > 0)
        {
            // If vehicle cannot proceed for a full second, halt current and further movement
            if (retries > 5) break;

            // Wait until all commands have been completed by the unit before issuing next move command
            if (_currentCommand != null)
            {
                yield return new WaitForSeconds(0.2f);
            }
            // If vehicle is in the way, check again in 0.5s and move accordingly
            if (Helper.IsVehicleInTheWay(nextGrid))
            {
                retries += 1;
                yield return new WaitForSeconds(0.2f);
            }
            else   // If coast is clear, issue the next movement command
            {
                GiveMovementCommand(nextMove);
                moveQueue.Dequeue();
                if (moveQueue.Count > 0)
                {
                    nextMove = moveQueue.Peek();
                    nextGrid = Helper.AddGridCoords(nextGrid, nextMove);
                }
            }
            yield return new WaitForSeconds(0.2f);
        }
        TurnInProgress = false;
        PostTurnActions();
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
        if (_currentGridPosition[0].y < dividerY)
        {
            return _currentGridPosition[0].x == 0;
        }
        else
        {
            return _currentGridPosition[0].x == FieldGrid.GetMaxLength() - 1;
        }
    }

    public bool IsBruteInTheWay(GridCoord targetGrid)
    {
        return FieldGrid.IsWithinField(targetGrid) && FieldGrid.GetSingleGrid(targetGrid).GetUnitsTag().Contains("Brute");
    }

    public void ReverseMotion()
    {
        movementPattern = movementPattern.Select(x => new GridCoord(x.x * -1, x.y)).ToList();
    }

    public void UpdateLaneSpeed(int newLaneSpeed)
    {
        laneSpeedAddition = newLaneSpeed - 1;
    }

    public Queue<GridCoord> UpdateMovementWithLaneSpeed(Queue<GridCoord> moveQueue)
    {
        for (int i = 0; i < laneSpeedAddition; i++)
        {
            if (moveQueue.Count >= maxSpeed)
            {
                break;
            }
            moveQueue.Enqueue(movementPattern.Last());
        }
        return moveQueue;
    }

    protected bool IsInLeftLane()
    {
        return _currentGridPosition[0].y < dividerY;
    }

    GridCoord GetNextBodyPosition(GridCoord position)
    {
        if (IsInLeftLane())
        {
            position.x += 1;
        }
        else
        {
            position.x -= 1;
        }
        return position;
    }
}