public abstract class Command
{
    public abstract void Execute(Unit unit);
    protected bool isFinished;
    public bool IsFinished { get => isFinished; protected set => isFinished = value; }
}