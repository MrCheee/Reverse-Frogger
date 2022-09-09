using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Unit : MonoBehaviour, IUnit
{
    [SerializeField] private float moveSpeed = 1f;

    public int health;
    protected int damage;
    public int skipTurn = 0;
    protected string unitTag;
    protected Queue<Command> commandStack = new Queue<Command>();
    protected Command _currentCommand;
    protected List<GridCoord> movementPattern = new List<GridCoord>();
    public float yAdjustment { get; protected set; }
    public bool TurnInProgress { get; protected set; }

    protected virtual void Awake()
    {
        // Each enemy will define its own movement pattern and it will be assigned to the private variable on startup
        SetHealthAndDamage();
        SetAdditionalTag();
        SetMovementPattern();
    }

    public void BeginTurn()
    {
        StartCoroutine("TakeTurn");
    }

    protected abstract void SetHealthAndDamage();
    protected abstract void SetAdditionalTag();
    public abstract void SetMovementPattern();
    public abstract GridCoord GetCurrentGridPosition();
    public abstract void AddToFieldGridPosition(GridCoord position);
    public abstract void RemoveFromFieldGridPosition();
    public abstract void SetCurrentGridPosition(GridCoord position);
    public abstract void PreTurnActions();
    public abstract IEnumerator TakeTurn();
    public abstract void PostTurnActions();
    public abstract void CheckConditionsToDestroy();
    public abstract void TakeVehicleInTheWayAction();
    public abstract void TakeNoVehicleInTheWayAction();
    public abstract bool HaltMovementByVehicleInTheWay();

    protected bool ToSkipTurn()
    {
        if (skipTurn > 0)
        {
            skipTurn -= 1;
            return true;
        }
        return false;
    }

    public bool IsObjInTheWay(GridCoord targetGrid)
    {
        return FieldGrid.IsWithinField(targetGrid) && FieldGrid.GetSingleGrid(targetGrid).GetUnitCount() > 0;
    }

    public bool IsVehicleInTheWay(GridCoord targetGrid)
    {
        return FieldGrid.IsWithinField(targetGrid) && FieldGrid.GetSingleGrid(targetGrid).GetUnitsTag().Contains("Vehicle");
    }

    public void Move(Vector3 moveDirection)
    {
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
    }

    public bool ReachedPosition(Vector3 target)
    {
        return Helper.vectorDistanceIgnoringYAxis(transform.position, target) <= 0.1f;
    }

    public void DestroySelf()
    {
        RemoveFromFieldGridPosition();
        Destroy(gameObject);
    }

    // [To Be Added] Update  will only run these command execution when in its correct game state
    // [Or each specific commands can only be run in certain game state]
    // Update will constantly check for commands to execute
    public void Update()
    {
        if (_currentCommand != null)
        {
            // Continue execution of current command if it is still uncompleted
            if (!_currentCommand.IsFinished)
            {
                _currentCommand.Execute(this);
            }
            else // Remove command from stack and set current command to null
            {
                //Debug.Log("Pop Command");
                commandStack.Dequeue();
                _currentCommand = null;
            }
        }
        else
        {
            CheckConditionsToDestroy();

            // If no current command, check if any in stack, grab the top command if there is
            if (commandStack.Count > 0)
            {
                //Debug.Log("Executing Command");
                _currentCommand = commandStack.Peek();
            }
        }
    }

    public void TakeDamage(int damageReceived)
    {
        health -= damageReceived;
        if (health <= 0)
        {
            DestroySelf();
        }
    }

    public void IssueCommand(Command cmd)
    {
        commandStack.Enqueue(cmd);
    }

    public void GiveMovementCommand(GridCoord moveGrid)
    {
        commandStack.Enqueue(new MoveToGridCommand(moveGrid));
    }

    public string GetTag()
    {
        return unitTag;
    }

    public int GetHealth()
    {
        return health;
    }

    public void AddDamage(int damageAdd)
    {
        damage += damageAdd;
        damage = Mathf.Max(damage, 0);
    }

    public void DisableUnit(int disableTime)
    {
        skipTurn += disableTime;
    }
}
