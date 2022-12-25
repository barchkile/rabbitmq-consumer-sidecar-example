using System;
using System.Collections.Generic;
using RabbitMQ.Client;

namespace RabbitMQ.Consumer;

public static class ChannelExtensions
{
    private const string DefaultExchangeName = "";

    public static void DeclareQueueByQueueConfig(this IModel channel, string queueName, QueueConfig queueConfig)
    {
        if (queueConfig == null)
        {
            throw new ArgumentNullException(nameof(queueConfig), $"Can't declare queue '{queueName}', queue config is missing");
        }

        var queueProperties = new Dictionary<string, object>();

        if (queueConfig.ForwardToDeadLetterOnFailure)
        {
            queueProperties[Headers.XDeadLetterExchange] = channel.DeclareDeadLetterExchange(
                queueName,
                queueConfig.Retryable,
                queueConfig.RetryDelaySeconds
            );
        }

        if (queueConfig.MaxPriority != null)
        {
            queueProperties[Headers.XMaxPriority] = queueConfig.MaxPriority;
        }

        channel.QueueDeclare(
            queueName,
            true,
            false,
            false,
            queueProperties
        );
    }

    private static string DeclareDeadLetterExchange(this IModel channel, string queueName, bool retryable, int retryDelaySeconds)
    {
        var deadLetterExchange = DeadLetterConventions.DeadLetterExchangeName(queueName);
        var deadLetterQueue = DeadLetterConventions.DeadLetterQueueName(queueName);
        channel.ExchangeDeclare(
            deadLetterExchange,
            ExchangeType.Fanout,
            true
        );

        var queueProperties = new Dictionary<string, object>();
        if (retryable)
        {
            queueProperties[Headers.XDeadLetterExchange] = DefaultExchangeName;
            queueProperties[Headers.XDeadLetterRoutingKey] = queueName;
            queueProperties[Headers.XMessageTTL] = retryDelaySeconds * 1000;
        }

        channel.QueueDeclare(
            deadLetterQueue,
            true,
            false,
            false,
            queueProperties
        );

        channel.QueueBind(
            deadLetterQueue,
            deadLetterExchange,
            ""
        );

        return deadLetterExchange;
    }
}