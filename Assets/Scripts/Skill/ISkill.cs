public interface ISkill
{
    Unit TargetUnit { get; set; }
    GridCoord TargetGrid { get; set; }
    public void Execute();
    public void UpdateGridCoordAction(GridCoord coord);
}