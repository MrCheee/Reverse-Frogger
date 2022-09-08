public class Vaulter : Enemy
{
    bool vaultAvailable = true;

    protected override void SetHealthAndDamage()
    {
        health = 1;
        damage = 1;
    }

    public override void SetMovementPattern()
    {
        movementPattern.Add(new GridCoord(0, 1));
    }

    public override void TakeVehicleInTheWayAction()
    {
        GridCoord currentGrid = GetCurrentGridPosition();
        GridCoord nextGrid = new GridCoord(currentGrid.x, currentGrid.y + 1);
        if (vaultAvailable)
        {
            vaultAvailable = false;
            GetComponentInChildren<VaultTrigger>().DestroySelf();
            commandStack.Enqueue(new MoveWithinGridCommand(FieldGrid.GetSingleGrid(nextGrid).GetCornerPoint(0, -1), 3));
            commandStack.Enqueue(new MoveWithinGridCommand(FieldGrid.GetSingleGrid(nextGrid).GetCornerPoint(0, 0), 3));
            commandStack.Enqueue(new MoveWithinGridCommand(FieldGrid.GetSingleGrid(nextGrid).GetCornerPoint(0, 1), 3));
            SetCurrentGridPosition(nextGrid);

            GridCoord nextnextGrid = new GridCoord(currentGrid.x, currentGrid.y + 1);
            if (Helper.IsVehicleInTheWay(nextnextGrid))
            {
                yAdjustment = 3;
            }
        }
        else
        {
            skipTurn = 1;

            commandStack.Enqueue(new MoveWithinGridCommand(FieldGrid.GetSingleGrid(currentGrid).GetCornerPoint(0, 1)));
            commandStack.Enqueue(new MoveWithinGridCommand(FieldGrid.GetSingleGrid(currentGrid).GetCornerPoint(0, 0)));
        }
    }

    public override void TakeNoVehicleInTheWayAction()
    {
        yAdjustment = 0;
    }

    public override bool HaltMovementByVehicleInTheWay()
    {
        return false;
    }

    public void VaultHit()
    {
        skipTurn = 1;
        vaultAvailable = false;
    }
}