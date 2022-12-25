using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace RabbitMQ.Consumer;

public class QueuesDefinitions
{
    public IReadOnlyDictionary<string, QueueConfig> Queues { get; init; } = new Dictionary<string, QueueConfig>();

    public IReadOnlyDictionary<string, ConsumerConfig> Consumers { get; } = new Dictionary<string, ConsumerConfig>();

    public static QueuesDefinitions LoadFromFile(string definitionsFilePath)
    {
        if (string.IsNullOrEmpty(definitionsFilePath) || !File.Exists(definitionsFilePath))
        {
            throw new ArgumentException($"Invalid definitions file path: '{definitionsFilePath}'");
        }

        return JsonConvert.DeserializeObject<QueuesDefinitions>(File.ReadAllText(definitionsFilePath));
    }
}