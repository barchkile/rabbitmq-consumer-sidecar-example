using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQ.Consumer;

public interface IQueueConsumer
{
    string QueueName { get; }

    Task StartAsync(CancellationToken cancellationToken);

    Task StopAsync(CancellationToken cancellationToken);
}