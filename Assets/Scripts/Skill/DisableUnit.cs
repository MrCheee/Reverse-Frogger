public class DisableUnit : ISkill
{
    public Unit unit { get; set; }
    public GridCoord targetGrid { get; set; }

    public DisableUnit(Unit target)
    {
        unit = target;
    }

    public void Execute()
    {
        unit.DisableUnit(2);
    }

    public void UpdateGridCoordAction(GridCoord coord)
    {
        return;
    }
}
