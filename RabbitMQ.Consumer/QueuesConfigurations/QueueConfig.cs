namespace RabbitMQ.Consumer;

public class QueueConfig
{
    public byte? MaxPriority { get; set; }

    public QueueFailurePolicy FailurePolicy { get; set; }

    public int RetryDelaySeconds { get; set; } = 60;

    public bool ForwardToDeadLetterOnFailure => FailurePolicy is QueueFailurePolicy.Retry or QueueFailurePolicy.DeadLetter;

    public bool Retryable => FailurePolicy == QueueFailurePolicy.Retry;
}