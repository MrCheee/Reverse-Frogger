public abstract class Command
{
    public abstract void Execute(Unit unit);
    public bool IsFinished { get; protected set; }
}