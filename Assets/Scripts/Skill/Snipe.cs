public class Snipe : ISkill
{
    public Unit unit { get; set; }
    public GridCoord targetGrid { get; set; }

    public Snipe(Unit target)
    {
        unit = target;
    }

    public void Execute()
    {
        unit.TakeDamage(1);
    }

    public void UpdateGridCoordAction(GridCoord coord)
    {
        return;
    }
}
