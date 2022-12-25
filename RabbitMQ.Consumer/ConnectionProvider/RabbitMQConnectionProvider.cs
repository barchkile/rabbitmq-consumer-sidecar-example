using RabbitMQ.Client;

namespace RabbitMQ.Consumer;

public class RabbitMQConnectionProvider : IRabbitMQConnectionProvider
{
    private readonly ConnectionFactory _factory;

    public RabbitMQConnectionProvider(string hostname, int port)
        => _factory = new ConnectionFactory
        {
            HostName = hostname,
            Port = port,
            AutomaticRecoveryEnabled = true
        };

    public IConnection GetConnection(string clientProvidedName)
        => _factory.CreateConnection(clientProvidedName);
}