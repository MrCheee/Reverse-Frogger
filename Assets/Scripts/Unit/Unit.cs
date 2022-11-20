using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Unit : MonoBehaviour, IUnit, UIMain.IUIInfoContent
{
    protected float moveSpeed = 7.5f;
    protected float deathTimer = 0f;

    protected int chargePerTurn = 0;
    protected List<SkillType> targeted = new List<SkillType>();
    protected GridCoord _currentGridPosition;

    protected Queue<Command> commandStack = new Queue<Command>();
    protected Command _currentCommand;
    public List<GridCoord> movementPattern = new List<GridCoord>();

    protected bool turnCoroutineInProgress = false;
    protected bool actionTaken = false;
    protected AudioSource audioSource;

    public int Health { get; protected set; }
    public int Damage { get; protected set; }
    public int SkipTurn { get; protected set; }
    public int Charging { get; protected set; }
    public float VerticalDisplacement { get; protected set; }
    public bool TurnInProgress { get => turnCoroutineInProgress && HasCompletedAllCommands(); protected set => turnCoroutineInProgress = value; }
    public string SpecialTag { get; protected set; }

    // Animation
    protected Animator animator;
    protected static readonly int movingAP = Animator.StringToHash("Moving");
    protected static readonly int stunnedAP = Animator.StringToHash("Stunned");
    protected static readonly int chargingAP = Animator.StringToHash("Charging");

    // IUnit Interface
    public abstract GridCoord GetCurrentHeadGridPosition();

    public abstract void UpdateGridMovement(GridCoord position);

    public void BeginPreTurn()
    {
        TurnInProgress = true;
        StartCoroutine("PreTurnActions");
    }

    public void BeginTurn()
    {
        TurnInProgress = true;
        StartCoroutine("TakeTurn");
    }

    public void BeginPostTurn()
    {
        TurnInProgress = true;
        StartCoroutine("PostTurnActions");
    }

    public void Move(Vector3 moveDirection)
    {
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
    }

    public void Rotate(Vector3 rotateAxis, float rotateAmt)
    {
        transform.RotateAround(transform.position, rotateAxis, rotateAmt);
    }

    public void TakeDamage(int damageReceived)
    {
        Health -= damageReceived;
        if (Health <= 0)
        {
            deathTimer = 1.5f;
            DestroySelf();
        }
    }

    public virtual void IssueCommand(Command cmd)
    {
        commandStack.Enqueue(cmd);
    }

    public void DisableUnit(int disableTime)
    {
        SkipTurn += disableTime;
        animator.SetBool(stunnedAP, true);
    }

    // UIMain.IUIInfoContent Interface
    public abstract string GetName();
    public abstract string GetDescription();
    public virtual void GetContent(ref List<Status> content)
    {
        content.Add(new Status("Health", Health));
        content.Add(new Status("Damage", Damage));
        content.Add(new Status("Stunned", SkipTurn));
        content.Add(new Status("Charging", Charging));
    }

    // Non-interface Abstract methods
    protected abstract void SetUnitAttributes();
    protected abstract void SetMovementPattern();
    protected abstract IEnumerator PreTurnActions();
    protected abstract IEnumerator TakeTurn();
    protected abstract IEnumerator PostTurnActions();
    protected abstract void CheckConditionsToDestroy();
    protected abstract void DestroySelf();
    

    protected virtual void Start()
    {
        audioSource = GetComponentInChildren<AudioSource>();
        animator = GetComponentInChildren<Animator>();
        SetUnitAttributes();
        SetMovementPattern();
    }

    // Update will constantly check for commands to execute
    private void Update()
    {
        if (Health <= 0) return;

        if (_currentCommand != null)
        {
            // Continue execution of current command if it is still uncompleted
            if (!_currentCommand.IsFinished)
            {
                _currentCommand.Execute(this);
            }
            else // Remove command from stack and set current command to null
            {
                commandStack.Dequeue();
                _currentCommand = null;

                // Only check after a command has been completed
                CheckConditionsToDestroy();
            }
        }
        else
        {
            // If no current command, check if any in stack, grab the top command if there is
            if (commandStack.Count > 0)
            {
                _currentCommand = commandStack.Peek();
            }
            else
            {
                if (!TurnInProgress)
                {
                    animator.SetBool(movingAP, false);
                }
            }
        }
    }

    protected bool HasCompletedAllCommands()
    {
        return commandStack.Count == 0;
    }

    protected bool ToSkipTurn()
    {
        if (SkipTurn > 0)
        {
            SkipTurn -= 1;
            if (SkipTurn <= 0)
            {
                animator.SetBool(stunnedAP, false);
            }
            TurnInProgress = false;
            return true;
        }
        return false;
    }

    protected bool StillChargingUp()
    {
        if (Charging > 0)
        {
            Charging -= 1;
            animator.SetTrigger(chargingAP);
            TurnInProgress = false;
            return true;
        }
        return false;
    }

    protected bool IsInLeftLane()
    {
        return _currentGridPosition.y < FieldGrid.DividerY;
    }

    public void AddTargetedSkill(SkillType skill)
    {
        targeted.Add(skill);
    }

    public void RemoveTargetedSkill(SkillType skill)
    {
        targeted.Remove(skill);
    }

    public int GetTargetedCount()
    {
        return targeted.Count;
    }
}
