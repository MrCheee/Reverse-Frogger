using UnityEngine;

public class Jumper : Enemy
{
    protected static readonly int jumpAP = Animator.StringToHash("Jump");

    protected override void SetUnitAttributes()
    {
        Health = 1;
        Damage = 1;
        chargePerTurn = 0;
        SpecialTag = "Roof-Ready";
    }

    protected override void SetMovementPattern()
    {
        movementPattern.Add(new GridCoord(0, direction));
    }

    protected override void TakeVehicleInTheWayAction()
    {
        VerticalDisplacement = 3;
        animator.SetTrigger(jumpAP);
        animator.SetBool(movingAP, false);
        commandStack.Enqueue(new MoveWithinGridCommand(FieldGrid.GetGrid(GetCurrentHeadGridPosition()).GetCornerPoint(0, direction)));
    }

    protected override void TakeNoVehicleInTheWayAction()
    {
        if (VerticalDisplacement == 3)
        {
            animator.SetTrigger(jumpAP);
            animator.SetBool(movingAP, false);
            commandStack.Enqueue(new MoveWithinGridCommand(FieldGrid.GetGrid(GetCurrentHeadGridPosition()).GetCornerPoint(0, direction), VerticalDisplacement));
        }
        else
        {
            animator.SetBool(movingAP, true);
        }
        VerticalDisplacement = 0;
    }

    protected override bool HaltMovementByVehicleInTheWay()
    {
        return false;
    }

    public override string GetName()
    {
        return "Imp";
    }

    public override string GetDescription()
    {
        return "Movement Pattern: <br>-Moves 1 step forward per turn. <br> <br>" +
            "Vehicle in the way: <br>-It will jump on top of the vehicle. <br> <br>" +
            "Additional effects: <br>-It will move along with the vehicle's movement while on top." +
            "<br>-While on top, it can hop onto another vehicle's roof.";
    }
}