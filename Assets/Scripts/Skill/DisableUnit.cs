public class DisableUnit : ISkill
{
    public Unit TargetUnit { get; set; }
    public GridCoord TargetGrid { get; set; }

    public DisableUnit(Unit target)
    {
        TargetUnit = target;
    }

    public void Execute()
    {
        TargetUnit.DisableUnit(1);
    }

    public void UpdateGridCoordAction(GridCoord coord)
    {
        return;
    }
}
