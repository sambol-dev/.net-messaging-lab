namespace Messaging.Worker.Exceptions;

public class RetryPublishException : Exception
{
    public RetryPublishException(
        string message,
        Exception? innerException = null)
        : base(message, innerException)
    {   
    }
}