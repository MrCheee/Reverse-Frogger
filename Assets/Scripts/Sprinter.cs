public class Sprinter : Enemy
{
    public override void SetMovementPattern()
    {
        movementPattern.Add(new GridCoord(0, 1));
        movementPattern.Add(new GridCoord(0, 1));
    }
}