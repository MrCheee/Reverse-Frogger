using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : Unit
{
    private List<Vehicle> Resist;
    protected GridCoord _currentGridPosition;
    protected int chargePerTurn = 0;
    public int charging = 0;

    protected override void Awake()
    {
        SetAdditionalTag();
        SetChargePerTurn();
        base.Awake();
    }

    protected override void SetAdditionalTag()
    {
        unitTag = "Normal";
    }
    protected virtual void SetChargePerTurn()
    {
        chargePerTurn = 0;
    }

    public override GridCoord GetCurrentGridPosition()
    {
        return _currentGridPosition;
    }

    public override void AddToFieldGridPosition(GridCoord position)
    {
        FieldGrid.GetSingleGrid(position).AddObject(gameObject);
        SetCurrentGridPosition(position);
    }

    public override void RemoveFromFieldGridPosition()
    {
        FieldGrid.GetSingleGrid(_currentGridPosition).RemoveObject(gameObject.GetInstanceID());
    }

    public override void SetCurrentGridPosition(GridCoord position)
    {
        _currentGridPosition = position;
    }

    // Turn ends when enemy has completed one cycle of its movement pattern
    // Or it is unable to complete its movement any further
    public override IEnumerator TakeTurn()
    {
        float retryInterval = 0.2f;

        if (ToSkipTurn()) yield break;
        if (StillChargingUp()) yield break;

        TurnInProgress = true;
        PreTurnActions();

        PrepareMovement(out Queue<GridCoord> moveQueue, out GridCoord nextMove, out GridCoord nextGrid);

        while (moveQueue.Count > 0)
        {
            // Halt all movement when enemy has crossed the road
            if (HasCrossedTheRoad()) break; 

            // Wait until all commands have been completed by the unit before issuing next move command
            if (!CheckIfCompletedPreviousMovement()) yield return new WaitForSeconds(retryInterval);

            // If vehicle is in the way, take role specific actions
            if (Helper.IsVehicleInTheWay(nextGrid))
            {
                TakeVehicleInTheWayAction();
                if (HaltMovementByVehicleInTheWay()) break;
            }
            else
            {
                TakeNoVehicleInTheWayAction();
            }
            
            // If not break away by vehicle in the way, issue the next movement command
            //Debug.Log($"Issue Move command of ({nextMove.x}, {nextMove.y}) to ({nextGrid.x}, {nextGrid.y})");
            (nextMove, nextGrid) = TakeMovementAction(moveQueue, nextMove, nextGrid);
            yield return new WaitForSeconds(0.2f);
        }
        TurnInProgress = false;
        PostTurnActions();
    }

    protected bool StillChargingUp()
    {
        if (charging > 0)
        {
            charging -= 1;
            return true;
        }
        return false;
    }

    protected void PrepareMovement(out Queue<GridCoord> moveQueue, out GridCoord nextMove, out GridCoord nextGrid)
    {
        moveQueue = new Queue<GridCoord>(movementPattern);
        nextMove = moveQueue.Peek();
        nextGrid = Helper.AddGridCoords(_currentGridPosition, nextMove);
    }

    protected bool CheckIfCompletedPreviousMovement()
    {
        return commandStack.Count == 0;
    }

    protected (GridCoord, GridCoord) TakeMovementAction(Queue<GridCoord> moveQueue, GridCoord nextMove, GridCoord nextGrid)
    {
        GiveMovementCommand(nextMove);
        moveQueue.Dequeue();
        if (moveQueue.Count > 0)
        {
            nextMove = moveQueue.Peek();
            nextGrid = Helper.AddGridCoords(nextGrid, nextMove);
        }
        return (nextMove, nextGrid);
    }

    public override void PreTurnActions()
    {
        return;
    }

    public override void PostTurnActions()
    {
        return;
    }

    public override void TakeVehicleInTheWayAction()
    {
        return;
    }

    public override void TakeNoVehicleInTheWayAction()
    {
        return;
    }

    public override bool HaltMovementByVehicleInTheWay()
    {
        return true;
    }

    public override void CheckConditionsToDestroy()
    {
        return;
    }

    public bool HasCrossedTheRoad()
    {
        return _currentGridPosition.y == FieldGrid.GetMaxHeight() - 1;
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        Debug.Log($"{gameObject.name}: Hit by a car! @({_currentGridPosition.x}, {_currentGridPosition.y})...");
        DestroySelf();
    }

    public void ChildTriggeredEnter(Collider other)
    {
        OnTriggerEnter(other);
    }
}