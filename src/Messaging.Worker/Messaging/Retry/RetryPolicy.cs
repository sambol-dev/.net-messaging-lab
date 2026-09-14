using Messaging.Shared.Messaging.Topology;

namespace Messaging.Worker.Messaging.Retry;

public class RetryPolicy : IRetryPolicy
{
    private const int FirstRetryDelaySeconds = 2;
    private const int SecondRetryDelaySeconds = 5;
    private const int ThirdRetryDelaySeconds = 10;
    public bool ShouldRetry(int retryCount)
    {
        return retryCount < RabbitMqTopology.MaxRetryAttempts;
    }
    public int GetNextRetryCount(int retryCount)
    {
        return retryCount + 1;
    }
    public TimeSpan GetRetryDelay(int retryCount)
    {
        var delay = retryCount switch
        {
            0 => TimeSpan.FromSeconds(FirstRetryDelaySeconds),
            1 => TimeSpan.FromSeconds(SecondRetryDelaySeconds),
            2 => TimeSpan.FromSeconds(ThirdRetryDelaySeconds),
            _ => TimeSpan.Zero
        };

        return delay;
    }
}