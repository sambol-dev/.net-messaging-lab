using Messaging.Shared.Messaging.Topology;

namespace Messaging.Worker.Messaging.Retry;

public class RetryPolicy : IRetryPolicy
{
    public bool ShouldRetry(int retryCount)
    {
        return retryCount < RabbitMqTopology.MaxRetryAttempts;
    }

    public int GetNextRetryCount(int retryCount)
    {
        return retryCount + 1;
    }
}