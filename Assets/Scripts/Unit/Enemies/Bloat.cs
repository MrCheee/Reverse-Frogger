using System.Collections.Generic;
using UnityEngine;

public class Bloat : Enemy
{
    protected static readonly int killedAP = Animator.StringToHash("Killed");

    protected override void SetUnitAttributes()
    {
        Health = 1;
        Damage = 2;
        chargePerTurn = 0;
        SpecialTag = "Bloat";
    }

    protected override void SetMovementPattern()
    {
        movementPattern.Add(new GridCoord(0, direction));
    }

    protected override void TakeVehicleInTheWayAction()
    {
        DisableUnit(1);
        ExecuteConcussedMovement();
    }

    protected override void DestroySelf()
    {
        animator.SetTrigger(killedAP);
        deathTimer = 2f;
        DisableVehiclesWithinRadius();
        base.DestroySelf();
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
                var vehiclesInGrid = FieldGrid.GetGrid(new GridCoord(currentX + i, currentY + j)).GetListOfUnitsWithGameObjectTag("Vehicle");
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