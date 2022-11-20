public class Snipe : ISkill
{
    public Unit TargetUnit { get; set; }
    public GridCoord TargetGrid { get; set; }

    public Snipe(Unit target)
    {
        TargetUnit = target;
    }

    public void Execute()
    {
        TargetUnit.TakeDamage(1);
    }

    public void UpdateGridCoordAction(GridCoord coord)
    {
        return;
    }
}
