public class Vaulter : Enemy
{
    bool vaultAvailable = true;

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
        movementPattern.Add(new GridCoord(0, 1));
    }

    public override void TakeVehicleInTheWayAction()
    {
        GridCoord currentGrid = GetCurrentGridPosition();
        GridCoord nextGrid = new GridCoord(currentGrid.x, currentGrid.y + 1);
        if (vaultAvailable)
        {
            GetComponentInChildren<VaultTrigger>().DestroySelf();

            GridCoord nextnextGrid = new GridCoord(currentGrid.x, currentGrid.y + 2);
            if (Helper.IsVehicleInTheWay(nextnextGrid))
            {
                yAdjustment = 3;
            }

            commandStack.Enqueue(new MoveWithinGridCommand(FieldGrid.GetSingleGrid(nextGrid).GetCornerPoint(0, -1), 3));
            commandStack.Enqueue(new MoveWithinGridCommand(FieldGrid.GetSingleGrid(nextGrid).GetCornerPoint(0, 0), 3));
            commandStack.Enqueue(new MoveWithinGridCommand(FieldGrid.GetSingleGrid(nextGrid).GetCornerPoint(0, 1), 3));
            RemoveFromFieldGridPosition();
            AddToFieldGridPosition(nextGrid);
        }
        else if (yAdjustment == 0)  // If not on another vehicle, then give "knocked" movement and skip turn
        {
            skipTurn = 1;

            commandStack.Enqueue(new MoveWithinGridCommand(FieldGrid.GetSingleGrid(currentGrid).GetCornerPoint(0, 1)));
            commandStack.Enqueue(new MoveWithinGridCommand(FieldGrid.GetSingleGrid(currentGrid).GetCornerPoint(0, 0)));
        }
    }

    public override void TakeNoVehicleInTheWayAction()
    {
        if (yAdjustment == 3)
        {
            commandStack.Enqueue(new MoveWithinGridCommand(FieldGrid.GetSingleGrid(GetCurrentGridPosition()).GetCornerPoint(0, 1), yAdjustment));
        }
        yAdjustment = 0;
    }

    public override bool HaltMovementByVehicleInTheWay()
    {
        if (vaultAvailable)   // If just vaulted, movement is not halted
        {
            vaultAvailable = false;  // Remove vaulting capability from unit
            return false;
        }
        else
        {                           // If already vaulted,
            if (yAdjustment == 3)   // and landed on another vehicle
            {
                return false;       // then movement is not halted, can jump on top of next vehicle in the way
            }
            else
            {
                return true;        // If not on another vehicle, then no move.
            }
        }
    }

    public void VaultHit()
    {
        skipTurn = 1;
        vaultAvailable = false;
    }
}