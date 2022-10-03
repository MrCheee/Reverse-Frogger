public class Assassinate : ISkill
{
    public Unit unit { get; set; }
    public GridCoord targetGrid { get; set; }

    public Assassinate(Unit target)
    {
        unit = target;
    }

    public void Execute()
    {
        unit.DestroySelf();
    }

    public void UpdateGridCoordAction(GridCoord coord)
    {
        return;
    }
}
