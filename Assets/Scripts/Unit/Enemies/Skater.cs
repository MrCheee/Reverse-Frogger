using System.Linq;

public class Skater : Enemy
{
    public override void SetMovementPattern()
    {
        movementPattern.Add(new GridCoord(1, 1));
    }
    public override void PostTurnActions()
    {
        ReverseMotion();
    }

    public void ReverseMotion()
    {
        movementPattern = movementPattern.Select(x => new GridCoord(x.x * -1, x.y)).ToList();
    }

}