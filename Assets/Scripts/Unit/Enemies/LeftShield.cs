using System.Collections;
using UnityEngine;

public class LeftShield : Soldier
{
    protected static readonly int blockAP = Animator.StringToHash("ToBlock");

    protected override void SetUnitAttributes()
    {
        Health = 1;
        Damage = 2;
        chargePerTurn = 0;
        SpecialTag = "LShield";
    }

    protected override IEnumerator PostTurnActions()
    {
        GetComponentInChildren<SpriteRenderer>().flipX = true;
        animator.SetBool(movingAP, false);
        int currentY = _currentGridPosition.y;
        if (currentY > FieldGrid.DividerY && currentY != FieldGrid.SidewalkTopY)
        {
            animator.SetTrigger(blockAP);
        }
        TurnInProgress = false;
        yield break;
    }

    public override string GetName()
    {
        return "Shield Warrior";
    }

    public override string GetDescription()
    {
        return "Shields left <br> <br> " +
            "Movement Pattern: <br>-Moves 1 step forward per turn. <br> <br>" +
            "Vehicle in the way: <br>-Runs into the vehicle and becomes stunned for 1 turn. <br> <br>" +
            "Additional effects: <br>-It will block against any vehicle coming from its shield direction, halting the vehicle in its track.";
    }
}
