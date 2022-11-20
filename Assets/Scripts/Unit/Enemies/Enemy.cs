using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : Unit
{
    public bool HasCrossed { get; protected set; }
    protected int direction = -1;

    public static event Action<Enemy> OnEnemyKilled;
    public static event Action OnCollision;
    public static event Action<int> DamagePlayer;

    protected virtual void TakeVehicleInTheWayAction()
    {
        return;
    }

    protected virtual void TakeNoVehicleInTheWayAction()
    {
        return;
    }

    protected virtual bool HaltMovementByVehicleInTheWay()
    {
        return true;
    }

    public override GridCoord GetCurrentHeadGridPosition()
    {
        return _currentGridPosition;
    }

    public override void UpdateGridMovement(GridCoord position)
    {
        RemoveFromFieldGridPosition();
        AddToFieldGridPosition(position);
    }

    public void AddToFieldGridPosition(GridCoord position)
    {
        FieldGrid.GetGrid(position).AddObject(gameObject);
        SetCurrentGridPosition(position);
    }

    public void RemoveFromFieldGridPosition()
    {
        FieldGrid.GetGrid(_currentGridPosition).RemoveObject(gameObject.GetInstanceID());
    }

    public void SetCurrentGridPosition(GridCoord position)
    {
        _currentGridPosition = position;
    }

    // Turn ends when enemy has completed one cycle of its movement pattern
    // Or it is unable to complete its movement any further
    protected override IEnumerator TakeTurn()
    {
        float retryInterval = 0.2f;

        if (HasCrossed)
        {
            TurnInProgress = false;
            yield break;
        }
        if (ToSkipTurn()) yield break;
        if (StillChargingUp()) yield break;

        animator.SetBool("Moving", true);
        actionTaken = true;

        PrepareMovement(out Queue<GridCoord> moveQueue, out GridCoord nextMove, out GridCoord nextGrid);

        while (moveQueue.Count > 0)
        {
            // Wait until all commands have been completed by the unit before issuing next move command
            if (!HasCompletedAllCommands()) yield return new WaitForSeconds(retryInterval);

            // If vehicle is in the way, take role specific actions
            if (FieldGrid.IsVehicleInTheWay(nextGrid))
            {
                TakeVehicleInTheWayAction();
                if (HaltMovementByVehicleInTheWay()) break;
            }
            else
            {
                TakeNoVehicleInTheWayAction();
            }

            // If not break away by vehicle in the way, issue the next movement command
            IssueCommand(new MoveToTargetGridCommand(nextGrid));

            // Halt further movement when enemy has crossed the road
            if (HasCrossedTheRoad(nextGrid))
            {
                TriggerDamageOnPlayer();
                MarkedAsCrossed();
                break;
            }

            (nextMove, nextGrid) = GetNextMovementAction(moveQueue, nextMove, nextGrid);

            yield return new WaitForSeconds(0.2f);
        }
        Charging = chargePerTurn;
        TurnInProgress = false;
    }

    protected void PrepareMovement(out Queue<GridCoord> moveQueue, out GridCoord nextMove, out GridCoord nextGrid)
    {
        moveQueue = new Queue<GridCoord>(movementPattern);
        nextMove = moveQueue.Peek();
        nextGrid = Helper.AddGridCoords(_currentGridPosition, nextMove);
    }
    
    protected (GridCoord, GridCoord) GetNextMovementAction(Queue<GridCoord> moveQueue, GridCoord nextMove, GridCoord nextGrid)
    {
        moveQueue.Dequeue();
        if (moveQueue.Count > 0)
        {
            nextMove = moveQueue.Peek();
            nextGrid = Helper.AddGridCoords(nextGrid, nextMove);
        }
        return (nextMove, nextGrid);
    }

    protected virtual void ExecuteConcussedMovement()
    {
        commandStack.Enqueue(new MoveWithinCurrentGridCommand(0, direction));
        commandStack.Enqueue(new MoveWithinCurrentGridCommand(0, 0));
    }

    protected override IEnumerator PreTurnActions()
    {
        TurnInProgress = false;
        yield break;
    }

    protected override IEnumerator PostTurnActions()
    {
        animator.SetBool(movingAP, false);
        TurnInProgress = false;
        yield break;
    }

    protected override void CheckConditionsToDestroy()
    {
        if (!FieldGrid.IsWithinPlayableField(GetCurrentHeadGridPosition()))
        {
            DestroySelf();
        }
    }

    protected void MarkedAsCrossed()
    {
        HasCrossed = true;
    }

    public bool HasCrossedTheRoad(GridCoord nextGrid)
    {
        if (direction < 0)
        {
            return nextGrid.y <= FieldGrid.SidewalkBottomY;
        }
        else
        {
            return nextGrid.y >= FieldGrid.SidewalkTopY;
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        OnCollision?.Invoke();
        DestroySelf();
    }

    protected override void DestroySelf()
    {
        Health = 0;
        OnEnemyKilled?.Invoke(this);
        Destroy(gameObject, deathTimer);
        RemoveFromFieldGridPosition();
    }

    public void ChildTriggeredEnter(Collider other)
    {
        OnTriggerEnter(other);
    }

    protected void TriggerDamageOnPlayer()
    {
        DamagePlayer?.Invoke(Damage);
    }
}