using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : Unit
{
    protected GridCoord _currentGridPosition;
    public bool Crossed { get; protected set; }
    protected int direction = -1;

    protected override void SetAdditionalTag()
    {
        unitTag = "Normal";
    }

    public override GridCoord GetCurrentHeadGridPosition()
    {
        return _currentGridPosition;
    }

    public override GridCoord[] GetAllCurrentGridPosition()
    {
        return new GridCoord[] { _currentGridPosition };
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

        if (Crossed)
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
            GiveMovementCommand(nextMove);

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
        charging = chargePerTurn;
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

    public override IEnumerator PreTurnActions()
    {
        TurnInProgress = false;
        yield break;
    }

    public override IEnumerator PostTurnActions()
    {
        animator.SetBool("Moving", false);
        TurnInProgress = false;
        yield break;
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
        if (!FieldGrid.IsWithinPlayableField(GetCurrentHeadGridPosition()))
        {
            DestroySelf();
        }
    }

    protected void MarkedAsCrossed()
    {
        Crossed = true;
    }

    public bool HasCrossedTheRoad(GridCoord nextGrid)
    {
        if (direction < 0)
        {
            return nextGrid.y <= FieldGrid.GetBottomSidewalkLaneNum();
        }
        else
        {
            return nextGrid.y >= FieldGrid.GetTopSidewalkLaneNum();
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        other.gameObject.GetComponentInParent<Vehicle>().Collided();
        gameStateManager.VehicleHit(other.gameObject.GetComponentInParent<Unit>());
        DestroySelf();
    }

    public override void DestroySelf()
    {
        health = 0;
        string killedInfo = $"{gameObject.GetComponent<Unit>().GetName()} has been killed at Grid [{_currentGridPosition.x}, {_currentGridPosition.y}]!";
        gameStateManager.EnemyKilled(transform.position, killedInfo); 
        RemoveFromFieldGridPosition();
        Destroy(gameObject, deathTimer);
    }

    public void ChildTriggeredEnter(Collider other)
    {
        OnTriggerEnter(other);
    }

    protected void TriggerDamageOnPlayer()
    {
        gameStateManager.DamagePlayer(damage);
    }
}