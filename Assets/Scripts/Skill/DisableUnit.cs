public class DisableUnit : Skill
{
    public DisableUnit(Unit target) : base(target) { }

    public override void Execute()
    {
        unit.DisableUnit(1);
    }
}
