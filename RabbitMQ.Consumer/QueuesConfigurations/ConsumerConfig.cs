using System.Collections.Generic;

namespace RabbitMQ.Consumer;

public record ConsumerConfig(IReadOnlyDictionary<string, QueueConsumerConfig> ConsumerQueues);