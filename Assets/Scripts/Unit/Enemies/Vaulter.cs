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
        GridCoord currentGrid = GetCurrentHeadGridPosition();
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
            commandStack.Enqueue(new MoveWithinGridCommand(FieldGrid.GetSingleGrid(GetCurrentHeadGridPosition()).GetCornerPoint(0, 1), yAdjustment));
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

    public override string GetName()
    {
        return "Vaulter";
    }

    public override string GetDescription()
    {
        return "Movement Pattern: Moves 1 step forward per turn. <br> <br>" +
            "Vehicle in the way: If vault pole is still available, it will vault over to the lane after, landing on the ground " +
            "or on top of the vehicle. If no pole is available, it will run into the vehicle and become stunned for 1 turn. <br> <br> " +
            "Additional effects: It can only vault once, thereafter losing its pole. Its pole extends one lane behind it, and " +
            "if a vehicle runs into the pole, it will destroy the pole and stun the vaulter for 1 turn.";
    }
}