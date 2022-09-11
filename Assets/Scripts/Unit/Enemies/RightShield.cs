public class RightShield : Soldier
{
    protected override void SetAdditionalTag()
    {
        unitTag = "RShield";
    }

    public override string GetName()
    {
        return "Shield (Right)";
    }

    public override string GetDescription()
    {
        return "Movement Pattern: Moves 1 step forward per turn. <br> <br>" +
            "Vehicle in the way: Runs into vehicle and become stunned for 1 turn. <br> <br>" +
            "Additional effects: It will block against any vehicle coming from its right, halting the vehicle in its track.";
    }
}