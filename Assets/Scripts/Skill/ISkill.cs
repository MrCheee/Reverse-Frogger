public interface ISkill
{
    Unit unit { get; set; }
    GridCoord targetGrid { get; set; }
    public void Execute();
    public void UpdateGridCoordAction(GridCoord coord);
}