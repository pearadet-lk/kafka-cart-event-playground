namespace BuildingBlocks.Observability;

public sealed class ResiliencePolicyOptions
{
    public int RetryCount { get; set; } = 3;
    public int BaseDelaySeconds { get; set; } = 2;
    public int CircuitBreakerFailures { get; set; } = 5;
    public int CircuitBreakerDurationSeconds { get; set; } = 30;
}
