public class LaneChange : ISkill
{
    public Unit TargetUnit { get; set; }
    public GridCoord TargetGrid { get; set; }

    public LaneChange(Unit target)
    {
        TargetUnit = target;
        TargetGrid = new GridCoord(0, 0); 
    }

    public void Execute()
    {
        TargetUnit.IssueCommand(new MoveToTargetGridCommand(TargetGrid));
    }

    public void UpdateGridCoordAction(GridCoord coord)
    {
        TargetGrid = coord;
    }
}
