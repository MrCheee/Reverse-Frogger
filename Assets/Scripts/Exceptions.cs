[System.Serializable]
public class AccessingFieldGridOutOfBoundsException : System.Exception
{
    public AccessingFieldGridOutOfBoundsException() { }
    public AccessingFieldGridOutOfBoundsException(string message) : base(message) { }
    public AccessingFieldGridOutOfBoundsException(string message, System.Exception inner) : base(message, inner) { }
    protected AccessingFieldGridOutOfBoundsException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}