using System.Collections;

public class Jumper : Enemy
{
    protected override void SetHealthAndDamage()
    {
        health = 1;
        damage = 1;
    }

    protected override void SetAdditionalTag()
    {
        unitTag = "Roof-Ready";
    }

    public override void SetMovementPattern()
    {
        movementPattern.Add(new GridCoord(0, direction));
    }

    public override void TakeVehicleInTheWayAction()
    {
        yAdjustment = 3;
        animator.SetTrigger("Jump");
        animator.SetBool("Moving", false);
        commandStack.Enqueue(new MoveWithinGridCommand(FieldGrid.GetSingleGrid(GetCurrentHeadGridPosition()).GetCornerPoint(0, direction)));
    }

    public override void TakeNoVehicleInTheWayAction()
    {
        if (yAdjustment == 3)
        {
            animator.SetTrigger("Jump");
            animator.SetBool("Moving", false);
            commandStack.Enqueue(new MoveWithinGridCommand(FieldGrid.GetSingleGrid(GetCurrentHeadGridPosition()).GetCornerPoint(0, direction), yAdjustment));
        }
        else
        {
            animator.SetBool("Moving", true);
        }
        yAdjustment = 0;
    }

    public override bool HaltMovementByVehicleInTheWay()
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