using System.Collections.Generic;
using UnityEngine;

public class Bloat : Enemy
{
    protected override void SetUnitAttributes()
    {
        health = 1;
        damage = 2;
        deathTimer = 2f;
        chargePerTurn = 0;
    }

    protected override void SetAdditionalTag()
    {
        unitTag = "Bloat";
    }

    public override void SetMovementPattern()
    {
        movementPattern.Add(new GridCoord(0, direction));
    }

    public override void TakeVehicleInTheWayAction()
    {
        DisableUnit(1);
        ExecuteConcussedMovement();
    }

    public override void DestroySelf()
    {
        health = 0;
        animator.SetTrigger("Killed");
        DisableVehiclesWithinRadius();
        string killedInfo = $"{gameObject.GetComponent<Unit>().GetName()} has been killed at Grid [{_currentGridPosition.x}, {_currentGridPosition.y}]!";
        gameStateManager.EnemyKilled(transform.position, killedInfo);
        RemoveFromFieldGridPosition();
        Destroy(gameObject, deathTimer);
    }

    private void DisableVehiclesWithinRadius()
    {
        int currentX = _currentGridPosition.x;
        int currentY = _currentGridPosition.y;
        HashSet<int> disabledVehiclesID = new HashSet<int>();

        // Get vehicles in each grid around the bloat, and disable it by 1. Use instance ID to ensure disable is only applied once per unit.
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                var vehiclesInGrid = FieldGrid.GetSingleGrid(new GridCoord(currentX + i, currentY + j)).GetListOfUnitsWithGameObjectTag("Vehicle");
                foreach (var veh in vehiclesInGrid)
                {
                    int id = veh.gameObject.GetInstanceID();
                    if (!disabledVehiclesID.Contains(id))
                    {
                        veh.DisableUnit(1);
                        disabledVehiclesID.Add(id);
                    }
                }
            }
        }
    }

    public override string GetName()
    {
        return "Bloat";
    }

    public override string GetDescription()
    {
        return "Movement Pattern: <br>-Moves 1 step forward per turn. <br> <br>" +
            "Vehicle in the way: <br>-Runs into the vehicle and becomes stunned for 1 turn. <br> <br>" +
            "Additional effects: <br>-When it is killed, it will explode toxic vapour in a 1 grid radius, " +
            "stopping all vehicles around it in their tracks and stunning them for 1 turn.";
    }
}