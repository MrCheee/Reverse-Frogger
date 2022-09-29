public class BoostUnit : Skill
{
    public BoostUnit(Unit target) : base(target) { }

    public override void Execute()
    {
        unit.BoostUnit(1);
    }
}
