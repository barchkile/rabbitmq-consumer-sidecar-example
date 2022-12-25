namespace RabbitMQ.Consumer;

public enum QueueFailurePolicy
{
    Discard,
    DeadLetter,
    Retry
}