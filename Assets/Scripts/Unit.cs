﻿using System.Collections.Generic;
using UnityEngine;

public abstract class Unit : MonoBehaviour, IUnit
{
    [SerializeField] private float moveSpeed = 1f;

    protected int health;
    protected int damage;
    protected Queue<Command> commandStack = new Queue<Command>();
    protected Command _currentCommand;
    protected List<GridCoord> movementPattern = new List<GridCoord>();
    protected GridCoord currentGridPosition;

    public void Awake()
    {
        // Each enemy will define its own movement pattern and it will be assigned to the private variable on startup
        SetMovementPattern();
        InvokeRepeating("TakeTurn", 3.0f, 3.0f);
    }

    // Define own movement pattern for each subclass
    public abstract void SetMovementPattern();
    public abstract void TakeTurn();
    public abstract void CheckConditionsToDestroy();

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
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);
    }

    public bool ReachedPosition(Vector3 target)
    {
        return Helper.vectorDistanceIgnoringYAxis(transform.position, target) <= 0.1f;
    }

    public void DestroySelf()
    {
        RemoveFromFieldGridPosition(currentGridPosition);
        Destroy(gameObject);
    }

    public void IssueCommand(Command cmd)
    {
        commandStack.Enqueue(cmd);
    }

    public void AddToFieldGridPosition(GridCoord position)
    {
        FieldGrid.GetSingleGrid(position).AddObject(gameObject);
        SetCurrentGridPosition(position);
    }

    public void RemoveFromFieldGridPosition(GridCoord position)
    {
        FieldGrid.GetSingleGrid(position).RemoveObject(gameObject.GetInstanceID());
    }

    public void SetCurrentGridPosition(GridCoord position)
    {
        currentGridPosition = position;
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
                Debug.Log("Pop Command");
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
                Debug.Log("Executing Command");
                _currentCommand = commandStack.Peek();
            }
        }
    }

    public void GiveMovementCommand(GridCoord startGrid, GridCoord moveGrid)
    {
        commandStack.Enqueue(new MoveToGridCommand(startGrid, moveGrid));
    }
}