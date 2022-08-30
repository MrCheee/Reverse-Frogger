using System.Collections.Generic;
using UnityEngine;

public abstract class Unit : MonoBehaviour, IUnit
{
    [SerializeField] private float moveSpeed = 1f;

    protected int health;
    protected int damage;
    protected Stack<Command> commandStack = new Stack<Command>();
    protected Command _currentCommand;
    protected List<GridCoord> movementPattern = new List<GridCoord>();
    protected GridCoord currentGridPosition;

    public void Awake()
    {
        // Each enemy will define its own movement pattern and it will be assigned to the private variable on startup
        SetMovementPattern();
        Invoke("TestSingleMovement", 1.0f);
    }

    // Define own movement pattern for each subclass
    public abstract void SetMovementPattern();

    public bool IsObjInTheWay(ISingleGrid target)
    {
        return target.GetUnitCount() > 0;
    }

    public string ObjTypeInTheWay(ISingleGrid target)
    {
        return target.GetUnitsTag();
    }

    public void Move(Vector3 target)
    {
        transform.Translate(target * moveSpeed * Time.deltaTime);
    }

    public bool ReachedPosition(Vector3 target)
    {
        return Vector3.Distance(transform.position, target) <= 1f;
    }
    
    public void IssueCommand(Command cmd)
    {
        commandStack.Push(cmd);
    }

    public void SetCurrentGridPosition(GridCoord position)
    {
        currentGridPosition = position;
    }

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
                commandStack.Pop();
                _currentCommand = null;
            }
        }
        else
        {
            // If no current command, check if any in stack, grab the top command if there is
            if (commandStack.Count > 0)
            {
                _currentCommand = commandStack.Peek();
            }
        }

    }

    public void TestSingleMovement()
    {
        commandStack.Push(new MoveCommand(currentGridPosition, movementPattern[0]));
    }
}
