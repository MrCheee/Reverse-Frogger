public class LaneChange : ISkill
{
    public Unit unit { get; set; }
    public GridCoord targetGrid { get; set; }

    public LaneChange(Unit target)
    {
        unit = target;
        targetGrid = new GridCoord(0, 0); 
    }

    public void Execute()
    {
        unit.GiveMovementCommand(targetGrid);
    }

    public void UpdateGridCoordAction(GridCoord coord)
    {
        targetGrid = coord;
    }
}
