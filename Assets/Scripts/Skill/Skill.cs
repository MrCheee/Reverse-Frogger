public abstract class Skill : ISkill
{
    public Unit unit { get; set; }

    protected Skill(Unit target)
    {
        unit = target;
    }

    public abstract void Execute();

    public virtual void UpdateGridCoordAction(GridCoord coord)
    {
        return;
    }
}
