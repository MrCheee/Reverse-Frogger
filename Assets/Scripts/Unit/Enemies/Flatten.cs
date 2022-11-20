using System.Collections;
using UnityEngine;

public class Flatten : Enemy
{
    protected static readonly int flattenAP = Animator.StringToHash("Flatten");
    protected static readonly int toFlattenAP = Animator.StringToHash("ToFlatten");

    protected override void SetUnitAttributes()
    {
        Health = 1;
        Damage = 1;
        chargePerTurn = 0;
        SpecialTag = "Basic";
    }

    protected override void SetMovementPattern()
    {
        movementPattern.Add(new GridCoord(0, direction));
    }

    protected override void TakeVehicleInTheWayAction()
    {
        VerticalDisplacement = -2f;
        commandStack.Enqueue(new MoveWithinGridCommand(FieldGrid.GetGrid(GetCurrentHeadGridPosition()).GetCornerPoint(0, direction)));
    }

    protected override void TakeNoVehicleInTheWayAction()
    {
        if (VerticalDisplacement == -2f)
        {
            animator.SetBool(flattenAP, false);
            commandStack.Enqueue(new MoveWithinGridCommand(FieldGrid.GetGrid(GetCurrentHeadGridPosition()).GetCornerPoint(0, direction), VerticalDisplacement));
        }
        VerticalDisplacement = 0;
    }

    protected override IEnumerator PreTurnActions()
    {
        if (HasCrossed || SkipTurn > 0 || Charging > 0)
        {
            TurnInProgress = false;
            yield break;
        }

        GridCoord nextMove = movementPattern[0];
        GridCoord nextGrid = Helper.AddGridCoords(_currentGridPosition, nextMove);
        if (FieldGrid.IsVehicleInTheWay(nextGrid))
        {
            if (!animator.GetBool(flattenAP))
            {
                animator.SetTrigger(toFlattenAP);
                animator.SetBool(flattenAP, true);
            }
        }
        TurnInProgress = false;
        yield break;
    }

    protected override bool HaltMovementByVehicleInTheWay()
    {
        return false;
    }

    public override string GetName()
    {
        return "Mutated Blood";
    }

    public override string GetDescription()
    {
        return "Movement Pattern: <br>-Moves 1 step forward per turn. <br> <br>" +
            "Vehicle in the way: <br>-It will flatten itself and move under the vehicle. <br> <br>" +
            "Additional effects: <br>-It will remain in its flattened state until it has moved out from under a vehicle.";
    }
}