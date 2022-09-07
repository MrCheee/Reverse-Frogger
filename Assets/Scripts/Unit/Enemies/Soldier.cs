public class Soldier : Enemy
{
    public override void SetMovementPattern()
    {
        movementPattern.Add(new GridCoord(0, 1));
    }
}