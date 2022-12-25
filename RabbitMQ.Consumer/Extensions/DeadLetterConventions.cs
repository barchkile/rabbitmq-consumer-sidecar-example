namespace RabbitMQ.Consumer;

public static class DeadLetterConventions
{
    public static string DeadLetterQueueName(string queueName)
        => $"{queueName}-dead-letter";

    public static string DeadLetterExchangeName(string queueName)
        => $"{queueName}.dead-letter.exchange";
}