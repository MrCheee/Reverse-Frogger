using System;

public class BoostUnit : ISkill
{
    public Unit TargetUnit { get; set; }
    public GridCoord TargetGrid { get; set; }
    public static event Action<Unit> BoostedUnit;

    public BoostUnit(Unit target)
    {
        TargetUnit = target;
    }

    public void Execute()
    {
        BoostedUnit?.Invoke(TargetUnit);
    }

    public void UpdateGridCoordAction(GridCoord coord)
    {
        return;
    }
}
