using System.Linq;

public class Car : Vehicle
{
    public override void SetMovementPattern()
    {
        movementPattern.Add(new GridCoord(1, 0));
    }
}