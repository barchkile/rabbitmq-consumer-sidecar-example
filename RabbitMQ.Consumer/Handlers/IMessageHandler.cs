using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQ.Consumer;

public interface IMessageHandler
{
    Task<bool> HandleMessageAsync(QueueMessage queueMessage, int handlingTimeoutInSeconds, CancellationToken cancellationToken);

    Task<bool> IsReadyAsync();
}