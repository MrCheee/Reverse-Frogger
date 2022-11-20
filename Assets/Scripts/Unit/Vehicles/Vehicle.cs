using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Vehicle : Unit
{
    public int Size { get; protected set; }
    protected GridCoord[] _currentOccupiedGrids;
    protected int maxSpeed = 1;
    private int laneSpeedAddition = 0;
    protected int dividerY;
    protected int moveDirection = 1;

    protected override void Start()
    {
        base.Start();
        dividerY = FieldGrid.DividerY;
        _currentOccupiedGrids = new GridCoord[Size];
        moveSpeed = 7.5f;
        chargePerTurn = 0;
        deathTimer = 1f;
    }

    protected override void SetUnitAttributes()
    {
        Health = 1;
        Damage = 1;
        Size = 1;
    }

    public override GridCoord GetCurrentHeadGridPosition()
    {
        return _currentGridPosition;
    }

    public GridCoord[] GetAllOccupiedGrids()
    {
        return _currentOccupiedGrids;
    }

    public override void UpdateGridMovement(GridCoord position)
    {
        RemoveFromFieldGridPosition(); 
        AddToFieldGridPosition(position);
    }

    public void AddToFieldGridPosition(GridCoord position)
    {
        SetCurrentGridPosition(position);
        for (int i = 0; i < Size; i++)
        {
            FieldGrid.GetGrid(_currentOccupiedGrids[i]).AddObject(gameObject);
        }
    }

    public void RemoveFromFieldGridPosition()
    {
        for (int i = 0; i < Size; i++)
        {
            FieldGrid.GetGrid(_currentOccupiedGrids[i]).RemoveObject(gameObject.GetInstanceID());
        }
    }

    public void SetCurrentGridPosition(GridCoord position)
    {
        _currentGridPosition = position;
        for (int i = 0; i < Size; i++)
        {
            _currentOccupiedGrids[i] = position;
            position = GetNextBodyPosition(position);
        }
    }

    protected override IEnumerator PreTurnActions()
    {
        yield break;
    }

    protected override IEnumerator TakeTurn()
    {
        int retries = 0;
        int maxRetries = 10;
        float retryInterval = 0.2f;

        if (ToSkipTurn()) yield break;

        animator.SetBool(movingAP, true);

        actionTaken = true;
        TurnInProgress = true;

        PrepareMovement(out Queue<GridCoord> moveQueue, out GridCoord nextMove, out GridCoord nextGrid);

        while (moveQueue.Count > 0)
        {
            // If vehicle cannot proceed for 2s, halt current and further movement
            if (retries > maxRetries) break;

            // Wait until all commands have been completed by the unit before issuing next move command
            if (!HasCompletedAllCommands()) yield return new WaitForSeconds(retryInterval);

            if (SkipTurn > 0)
            {
                break;
            }

            // If vehicle is in the way, check again in 0.2s up to a max retry count
            if (FieldGrid.IsVehicleInTheWay(nextGrid))
            {
                retries += 1;
            }
            else   // Check if coast is clear, and if so, issue the next movement command
            {
                if (IsShieldBlockingTheWay(nextMove, nextGrid)) // If Shield is blocking, halt current and further movement
                {
                    SimulateHitObstacle();
                    break;
                }
                if (IsEnemyTypeInTheWay(nextGrid, "Brute"))  // If Brute in the way, check if movement will kill it. If yes, proceed as normal.
                {
                    Brute brute = GetUnitInTheWay(nextGrid, "Brute") as Brute;
                    if (IsBruteTooStrong(brute))  // If Brute has more HP than vehicle damage
                    {
                        HandleBruteInTheWay(brute);  // Deal vehicle damage to brute, then halt movement
                        break;
                    }
                }
                (nextMove, nextGrid) = TakeMovementAction(moveQueue, nextMove, nextGrid);
                Debug.Log($"{nextMove.x}, {nextMove.y}");
            }
            yield return new WaitForSeconds(0.2f);
        }
        TurnInProgress = false;
        animator.SetBool(movingAP, false);
    }

    protected override IEnumerator PostTurnActions()
    {
        yield break;
    }

    void PrepareMovement(out Queue<GridCoord> moveQueue, out GridCoord nextMove, out GridCoord nextGrid)
    {
        moveQueue = new Queue<GridCoord>(movementPattern);
        moveQueue = UpdateMovementWithLaneSpeed(moveQueue);
        nextMove = moveQueue.Peek();
        nextGrid = Helper.AddGridCoords(_currentGridPosition, nextMove);
    }

    (GridCoord, GridCoord) TakeMovementAction(Queue<GridCoord> moveQueue, GridCoord nextMove, GridCoord nextGrid)
    {
        IssueCommand(new MoveToTargetGridCommand(nextGrid));
        moveQueue.Dequeue();
        if (moveQueue.Count > 0)
        {
            nextMove = moveQueue.Peek();
            nextGrid = Helper.AddGridCoords(nextGrid, nextMove);
        }
        return (nextMove, nextGrid);
    }

    public override void IssueCommand(Command cmd)
    {
        commandStack.Enqueue(cmd);
        if (cmd is MoveToTargetGridCommand moveCmd)
        {
            MoveUnitsOnTopOfVehicle(moveCmd.TargetGrid);
        }
    }

    public virtual bool IsBruteTooStrong(Brute brute)
    {
        return brute.Health > Damage;
    }

    public virtual void HandleBruteInTheWay(Brute brute)
    {
        brute.TakeDamage(Damage);
        SimulateHitObstacle();
    }

    protected void SimulateHitObstacle()
    {
        GridCoord currentGrid = GetCurrentHeadGridPosition();
        float right = currentGrid.y < dividerY ? -0.75f : 0.75f;
        commandStack.Enqueue(new MoveWithinCurrentGridCommand(right, 0));
        commandStack.Enqueue(new MoveWithinCurrentGridCommand(0, 0));
    }

    protected override void CheckConditionsToDestroy()
    {
        if (HasReachedEndOfRoad())
        {
            DestroySelf();
        }
    }

    protected override void DestroySelf()
    {
        RemoveFromFieldGridPosition();
        Health = 0;
        Destroy(gameObject, deathTimer);
    }

    public bool HasReachedEndOfRoad()
    {
        if (_currentGridPosition.y < dividerY)
        {
            return _currentGridPosition.x == 0;
        }
        else
        {
            return _currentGridPosition.x == FieldGrid.FieldLength - 1;
        }
    }

    public bool IsEnemyTypeInTheWay(GridCoord targetGrid, string enemyType)
    {
        return FieldGrid.IsWithinField(targetGrid) && FieldGrid.GetGrid(targetGrid).IsUnitTagInGrid(enemyType);
    }

    public Unit GetUnitInTheWay(GridCoord targetGrid, string enemyType)
    {
        return FieldGrid.GetGrid(targetGrid).GetUnitWithTag(enemyType);
    }

    public List<Unit> GetUnitsInTheWay(GridCoord targetGrid, string enemyType)
    {
        return FieldGrid.GetGrid(targetGrid).GetListOfUnitsWithTag(enemyType);
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

    public void MoveUnitsOnTopOfVehicle(GridCoord targetGrid)
    {
        for (int i = 0; i < Size; i++)
        {
            List<Unit> enemiesOnTop = FieldGrid.GetGrid(_currentOccupiedGrids[i]).GetListOfUnitsWithTag("Roof-Ready");
            foreach (Unit enemy in enemiesOnTop)
            {
                enemy.IssueCommand(new MoveToTargetGridCommand(targetGrid));
            }
        }
    }

    public void ReverseMotion()
    {
        moveDirection = -moveDirection;
    }

    public void UpdateLaneSpeedAddition(int newLaneSpeed)
    {
        laneSpeedAddition = newLaneSpeed - 1;
    }

    protected Queue<GridCoord> UpdateMovementWithLaneSpeed(Queue<GridCoord> moveQueue)
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
        Vector3 targetPos = FieldGrid.GetGrid(GetCurrentHeadGridPosition()).GetGridCentrePoint();
        Vector3 moveDirection;
        while (Vector3.Distance(targetPos, transform.position) > 0.2f)
        {
            moveDirection = (targetPos - transform.position).normalized;
            transform.Translate(moveDirection * 50 * Time.deltaTime, Space.World);
            yield return null;
        }
        GameObject.Find("AirdropTarget").GetComponent<AirdropTarget>().Trigger(GetName(), transform.position, -movementPattern[0].x);
    }
}
