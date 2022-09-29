public class LaneChange : Skill
{
    public LaneChange(Unit target) : base(target) { nextGrid = new GridCoord(0, 0); }

    public GridCoord nextGrid { get; set; }

    public override void Execute()
    {
        unit.GiveMovementCommand(nextGrid);
    }

    public override void UpdateGridCoordAction(GridCoord coord)
    {
        nextGrid = coord;
    }
}
