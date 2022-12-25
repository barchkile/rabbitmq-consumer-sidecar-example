using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RabbitMQ.Consumer;

public class MessageConsoleWriter : IMessageHandler
{
    public Task<bool> HandleMessageAsync(QueueMessage queueMessage, int handlingTimeoutInSeconds, CancellationToken cancellationToken)
    {
        Console.WriteLine($"{DateTime.UtcNow:u} | {JsonConvert.SerializeObject(queueMessage)}");
        return Task.FromResult(true);
    }

    public Task<bool> IsReadyAsync() => Task.FromResult(true);
}