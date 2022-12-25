using System;
using System.Collections.Generic;
using RabbitMQ.Client;

namespace RabbitMQ.Consumer;

public record QueueMessage(string QueueName, string Data, byte? Priority, long RetryCount, IReadOnlyDictionary<string, string> Headers, string ConsumeId)
{
    public static QueueMessage FromBasicProperties(IBasicProperties basicProperties, ReadOnlyMemory<byte> body, string queueName)
        => new(
            queueName,
            body.ToArray()
                .ToUtf8(),
            basicProperties?.Priority,
            basicProperties.DeathCount(),
            basicProperties.NonRabbitHeaders(),
            Guid.NewGuid()
                .ToString()
        );
}