public interface ISkill
{
    Unit unit { get; set; }

    public abstract void Execute();
    public abstract void UpdateGridCoordAction(GridCoord coord);
}