using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Unit : MonoBehaviour, IUnit, UIMain.IUIInfoContent
{
    [SerializeField] private float moveSpeed = 1f;

    protected int size = 1;
    protected int health = 0;
    protected int damage = 0;
    protected int skipTurn = 0;
    protected int chargePerTurn = 0;
    protected int charging = 0;
    protected int boosted = 0;

    protected string unitTag;
    protected Queue<Command> commandStack = new Queue<Command>();
    protected Command _currentCommand;
    protected List<GridCoord> movementPattern = new List<GridCoord>();
    protected GameStateManager gameStateManager;
    public float yAdjustment { get; protected set; }
    public bool TurnInProgress { get; protected set; }

    protected virtual void Awake()
    {
        gameStateManager = GameObject.Find("MainManager").GetComponent<GameStateManager>();

        // Each enemy will define its own movement pattern and it will be assigned to the private variable on startup
        SetHealthAndDamage();
        SetAdditionalTag();
        SetChargePerTurn();
        SetMovementPattern();
    }

    public void BeginTurn()
    {
        TurnInProgress = true;
        StartCoroutine("TakeTurn");
    }

    protected abstract void SetHealthAndDamage();
    protected abstract void SetAdditionalTag();
    protected abstract void SetChargePerTurn();
    public abstract void SetMovementPattern();
    public abstract GridCoord GetCurrentHeadGridPosition();
    public abstract GridCoord[] GetAllCurrentGridPosition();
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
            TurnInProgress = false;
            return true;
        }
        return false;
    }

    protected bool StillChargingUp()
    {
        if (charging > 0)
        {
            charging -= 1;
            TurnInProgress = false;
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
        return FieldGrid.IsWithinField(targetGrid) && FieldGrid.GetSingleGrid(targetGrid).GetListOfUnitsGameObjectTag().Contains("Vehicle");
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
        Debug.Log($"[{gameObject.name}] Destroying self...");
        health = 0;
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

    public virtual void GiveMovementCommand(GridCoord moveGrid)
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

    public int GetSize()
    {
        return size;
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

    public void BoostUnit(int boostCount)
    {
        boosted += boostCount;
    }

    public void ReduceBoostOnUnit()
    {
        boosted = Mathf.Max(0, boosted - 1);
    }

    public bool isBoosted()
    {
        return boosted > 0;
    }

    public bool isStunned()
    {
        return skipTurn > 0;
    }

    public virtual string GetName()
    {
        return "Default Name";
    }

    public virtual string GetDescription()
    {
        return "Default Description";
    }

    public virtual void GetContent(ref List<Status> content)
    {
        content.Add(new Status("Health", health));
        content.Add(new Status("Stunned", skipTurn));
        content.Add(new Status("Charging", charging));
    }
}
