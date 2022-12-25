using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace RabbitMQ.Consumer;

public class QueueConsumersManager : IHostedService
{
    private readonly List<IQueueConsumer> _queueConsumers;

    public QueueConsumersManager(IEnumerable<IQueueConsumer> queueConsumers)
        => _queueConsumers = queueConsumers.ToList();

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var queueConsumer in _queueConsumers)
        {
            await queueConsumer.StartAsync(cancellationToken);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var queueConsumer in _queueConsumers)
        {
            try
            {
                await queueConsumer.StopAsync(cancellationToken);
            }
            catch
            {
                // ignored
            }
        }
    }
}