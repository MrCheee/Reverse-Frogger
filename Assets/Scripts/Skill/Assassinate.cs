public class Assassinate : Skill
{
    public Assassinate(Unit target) : base(target) { }

    public override void Execute()
    {
        unit.DestroySelf();
    }
}
