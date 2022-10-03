public class BoostUnit : ISkill
{
    public Unit unit { get; set; }
    public GridCoord targetGrid { get; set; }

    public BoostUnit(Unit target)
    {
        unit = target;
    }

    public void Execute()
    {
        unit.BoostUnit(1);
    }

    public void UpdateGridCoordAction(GridCoord coord)
    {
        return;
    }
}
