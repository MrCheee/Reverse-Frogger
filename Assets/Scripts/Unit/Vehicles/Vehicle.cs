using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Vehicle : Unit
{
    public int SpeedAddition { get; set; }

    protected int maxSpeed = 1;

    private int laneSpeedAddition = 0;
    protected int dividerY = FieldGrid.GetFieldBuffer() + 1 + FieldGrid.GetNumberOfLanes();
    protected GridCoord[] _currentGridPosition;

    protected override void Awake()
    {
        SetUpSize();
        _currentGridPosition = new GridCoord[size];
        _currentGridPosition[0] = new GridCoord(dividerY, dividerY);
        moveSpeed = 7.5f;
        chargePerTurn = 0;
        deathTimer = 1f;
        base.Awake();
    }

    protected override void SetUnitAttributes()
    {
        health = 1;
        damage = 1;
    }

    protected abstract void SetUpSize();

    protected override void SetAdditionalTag()
    {
        unitTag = "Normal";
    }

    public override GridCoord GetCurrentHeadGridPosition()
    {
        return _currentGridPosition[0];
    }

    public override GridCoord[] GetAllCurrentGridPosition()
    {
        return _currentGridPosition;
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

    public override IEnumerator PreTurnActions()
    {
        yield break;
    }
    public override IEnumerator PostTurnActions()
    {
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

    public override IEnumerator TakeTurn()
    {
        int retries = 0;
        bool quickExit = false;
        int maxRetries = 10;
        float retryInterval = 0.2f;

        if (ToSkipTurn()) yield break;

        animator.SetBool("Moving", true);

        actionTaken = true;
        TurnInProgress = true;

        PrepareMovement(out Queue<GridCoord> moveQueue, out GridCoord nextMove, out GridCoord nextGrid);

        while (moveQueue.Count > 0)
        {
            // If vehicle cannot proceed for a full second, halt current and further movement
            if (retries > maxRetries) break;

            // Wait until all commands have been completed by the unit before issuing next move command
            if (!CheckIfCompletedPreviousMovement()) yield return new WaitForSeconds(retryInterval);

            if (skipTurn > 0)
            {
                break;
            }

            // If vehicle is in the way, check again in 0.5s and move accordingly
            if (Helper.IsVehicleInTheWay(nextGrid))
            {
                retries += 1;
            }
            else   // If coast is clear, issue the next movement command
            {
                if (IsShieldBlockingTheWay(nextMove, nextGrid)) // If Shield is blocking, halt current and further movement
                {
                    SimulateHitObstacle();
                    break;
                }
                if (IsEnemyTypeInTheWay(nextGrid, "Brute"))  // If Brute in the way, check if movement will kill it. If yes, proceed as normal.
                {
                    Unit brute = GetUnitInTheWay(nextGrid, "Brute");
                    if (IsBruteTooStrong(brute))  // If Brute has more HP than vehicle damage
                    {
                        HandleBruteInTheWay(brute);  // Deal vehicle damage to brute, then halt movement
                        break;
                    }
                }
                if (IsEnemyTypeInTheWay(nextGrid, "Bloat"))
                {
                    quickExit = true;
                }
                (nextMove, nextGrid) = TakeMovementAction(moveQueue, nextMove, nextGrid);
            }
            if (quickExit) break;
            yield return new WaitForSeconds(0.2f);
        }
        TurnInProgress = false;
        animator.SetBool("Moving", false);
    }

    void PrepareMovement(out Queue<GridCoord> moveQueue, out GridCoord nextMove, out GridCoord nextGrid)
    {
        moveQueue = new Queue<GridCoord>(movementPattern);
        moveQueue = UpdateMovementWithLaneSpeed(moveQueue);
        nextMove = moveQueue.Peek();
        nextGrid = Helper.AddGridCoords(_currentGridPosition[0], nextMove);
    }

    (GridCoord, GridCoord) TakeMovementAction(Queue<GridCoord> moveQueue, GridCoord nextMove, GridCoord nextGrid)
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

    public override void GiveMovementCommand(GridCoord moveGrid)
    {
        MoveUnitsOnTopOfVehicle(moveGrid);
        commandStack.Enqueue(new MoveToGridCommand(moveGrid));
    }

    public virtual void HandleBruteInTheWay(Unit brute)
    {
        brute.TakeDamage(damage);
        TakeDamage(1);
        SimulateHitObstacle();
    }

    protected void SimulateHitObstacle()
    {
        GridCoord currentGrid = GetCurrentHeadGridPosition();
        int right = currentGrid.y < dividerY ? -1 : 1;
        commandStack.Enqueue(new MoveWithinCurrentGridCommand(right, 0));
        commandStack.Enqueue(new MoveWithinCurrentGridCommand(0, 0));
    }

    public override void CheckConditionsToDestroy()
    {
        if (HasReachedEndOfRoad())
        {
            DestroySelf();
        }
    }

    public override void DestroySelf()
    {
        RemoveFromFieldGridPosition();
        if (health == 0)
        {
            animator.SetTrigger("Explode");
            deathTimer = 1.5f;
        }
        Destroy(gameObject, deathTimer);
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

    public bool IsEnemyTypeInTheWay(GridCoord targetGrid, string enemyType)
    {
        return FieldGrid.IsWithinField(targetGrid) && FieldGrid.GetSingleGrid(targetGrid).IsUnitTagInGrid(enemyType);
    }

    public Unit GetUnitInTheWay(GridCoord targetGrid, string enemyType)
    {
        return FieldGrid.GetSingleGrid(targetGrid).GetUnitWithTag(enemyType);
    }

    public List<Unit> GetUnitsInTheWay(GridCoord targetGrid, string enemyType)
    {
        return FieldGrid.GetSingleGrid(targetGrid).GetListOfUnitsWithTag(enemyType);
    }

    public virtual bool IsBruteTooStrong(Unit brute)
    {
        return brute.GetHealth() > damage;
    }

    public bool IsShieldBlockingTheWay(GridCoord nextMove, GridCoord nextGrid)
    {
        bool isBlocked = false;
        if (nextMove.x < 0)
        {
            isBlocked = IsEnemyTypeInTheWay(nextGrid, "RShield");
        }
        else if (nextMove.x > 0)
        {
            isBlocked = IsEnemyTypeInTheWay(nextGrid, "LShield");
        }
        return isBlocked;
    }

    public void MoveUnitsOnTopOfVehicle(GridCoord nextMove)
    {
        for (int i = 0; i < _currentGridPosition.Length; i++)
        {
            List<Unit> enemiesOnTop = FieldGrid.GetSingleGrid(_currentGridPosition[i]).GetListOfUnitsWithTag("Roof-Ready");
            foreach (Unit enemy in enemiesOnTop)
            {
                enemy.IssueCommand(new MoveToGridCommand(nextMove));
            }
        }
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

    public void SimulateAirdrop()
    {
        StartCoroutine("AirdropMotion");
    }

    public IEnumerator AirdropMotion()
    {
        Vector3 targetPos = FieldGrid.GetSingleGrid(GetCurrentHeadGridPosition()).GetGridCentrePoint();
        Vector3 moveDirection = (targetPos - transform.position).normalized;
        while (Vector3.Distance(targetPos, transform.position) > 0.5f)
        {
            transform.Translate(moveDirection * 50 * Time.deltaTime, Space.World);
            yield return null;
        }
    }
}