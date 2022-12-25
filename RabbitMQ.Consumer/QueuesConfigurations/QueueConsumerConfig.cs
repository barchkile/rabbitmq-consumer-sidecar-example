namespace RabbitMQ.Consumer;

public record QueueConsumerConfig(ushort MaxConcurrentConsumes = 4, int HandlingTimeoutInSeconds = 0);