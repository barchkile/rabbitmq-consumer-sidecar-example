namespace RabbitMQ.Consumer;

public record HandlerQueueMessageRequestBody(QueueMessage QueueMessage, int TimeoutInSeconds);