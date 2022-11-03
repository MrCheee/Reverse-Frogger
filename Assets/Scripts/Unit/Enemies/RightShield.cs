using System.Collections;
using UnityEngine;

public class RightShield : Soldier
{
    protected override void SetHealthAndDamage()
    {
        health = 1;
        damage = 2;
    }

    public override IEnumerator PostTurnActions()
    {
        GetComponentInChildren<SpriteRenderer>().flipX = false;
        animator.SetBool("Moving", false);
        int currentY = _currentGridPosition.y;
        if (currentY < FieldGrid.GetDividerLaneNum() && currentY != FieldGrid.GetBottomSidewalkLaneNum())
        {
            animator.SetTrigger("Block");
        }
        TurnInProgress = false;
        yield break;
    }

    protected override void SetAdditionalTag()
    {
        unitTag = "RShield";
    }

    public override string GetName()
    {
        return "Shark Soldier";
    }

    public override string GetDescription()
    {
        return "Shields right <br> <br> " +
            "Movement Pattern: <br>-Moves 1 step forward per turn. <br> <br>" +
            "Vehicle in the way: <br> -Runs into the vehicle and becomes stunned for 1 turn. <br> <br>" +
            "Additional effects: <br> -It will block against any vehicle coming from its shield direction, halting the vehicle in its track.";
    }
}