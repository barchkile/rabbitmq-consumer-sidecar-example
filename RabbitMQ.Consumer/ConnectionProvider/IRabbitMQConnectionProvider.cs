using RabbitMQ.Client;

namespace RabbitMQ.Consumer;

public interface IRabbitMQConnectionProvider
{
    IConnection GetConnection(string clientProvidedName);
}