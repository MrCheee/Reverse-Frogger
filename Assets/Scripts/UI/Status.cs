public struct Status
{
    public string statusType;
    public int count;

    public Status(string newType, int newCount)
    {
        statusType = newType;
        count = newCount;
    }
}