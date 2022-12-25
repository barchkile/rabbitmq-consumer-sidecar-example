using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RabbitMQ.Consumer;

public static class DependencyInjectionExtensions
{
    public static QueuesDefinitions AddCommonRabbitMQServices(this IServiceCollection services, IConfiguration configuration)
    {
        var rabbitHostName = configuration.GetValue("RABBITMQ_HOSTNAME", "rabbitmq");
        var rabbitPort = configuration.GetValue("RABBITMQ_PORT", 5672);
        var queuesDefinitionsPath = configuration.GetValue("QUEUES_DEFINITIONS_FILE_PATH", "/var/queues-definitions/queues-definitions.json");
        
        services.AddSingleton<IRabbitMQConnectionProvider>(new RabbitMQConnectionProvider(rabbitHostName, rabbitPort));

        var queuesDefinitions = QueuesDefinitions.LoadFromFile(queuesDefinitionsPath);
        services.AddSingleton(queuesDefinitions);

        return queuesDefinitions;
    }
}