namespace Messaging.Worker.Messaging.Retry;

public interface IRetryPolicy
{
    bool ShouldRetry(int retryCount);
    int GetNextRetryCount(int retryCount);
    TimeSpan GetRetryDelay(int retryCount);
}