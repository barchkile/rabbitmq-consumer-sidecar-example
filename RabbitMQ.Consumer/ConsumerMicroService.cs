using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace RabbitMQ.Consumer;

public class ConsumerMicroService
{
    public async Task RunAsync()
    {
        var host = new HostBuilder()
            .ConfigureAppConfiguration(configBuilder => configBuilder.AddEnvironmentVariables())
            .ConfigureServices((hostContext, services) => ConfigureServices(hostContext.Configuration, services))
            .UseConsoleLifetime()
            .Build();
        await host.RunAsync();
    }
    
    private void ConfigureServices(IConfiguration configuration, IServiceCollection services)
    {
        var queuesDefinitions = services.AddCommonRabbitMQServices(configuration);
        ConfigureDispatcher(configuration, services);
        ConfigureRabbitMQConsumer(
            configuration,
            services,
            queuesDefinitions
        );
    }

    private static void ConfigureRabbitMQConsumer(IConfiguration configuration, IServiceCollection services, QueuesDefinitions queuesDefinitions)
    {
        var mainServiceName = configuration.GetValue("MAIN_SERVICE_NAME", "");
        if (string.IsNullOrEmpty(mainServiceName))
        {
            Console.WriteLine("Missing 'MAIN_SERVICE_NAME' env, existing...");
            Environment.FailFast("Missing 'MAIN_SERVICE_NAME' env");
        }

        foreach (var (queueName, consumerConfig) in queuesDefinitions.Consumers[mainServiceName]
                     .ConsumerQueues)
        {
            services.AddSingleton<IQueueConsumer>(
                provider => new QueueConsumer(
                    queueName,
                    consumerConfig,
                    provider.GetRequiredService<QueuesDefinitions>(),
                    provider.GetRequiredService<IRabbitMQConnectionProvider>(),
                    provider.GetRequiredService<IMessageHandler>()
                )
            );
        }

        services.AddHostedService<QueueConsumersManager>();
    }

    private static void ConfigureDispatcher(IConfiguration configuration, IServiceCollection services)
    {
        var dispatcherType = configuration.GetValue("DISPATCHER_TYPE", "http");
        Console.WriteLine($"Initializing Dispatcher of type {dispatcherType}");
        switch (dispatcherType.ToLower())
        {
            case "console":
                services.AddSingleton<IMessageHandler, MessageConsoleWriter>();
                break;
            case "http":
            case "https":
                services.AddSingleton<IMessageHandler>(
                    new HttpMessageDispatcher(
                        new HttpMessageDispatcherConfig(
                            configuration.GetValue("HTTP_DISPATCHER_BASE_URL", "http://localhost"),
                            configuration.GetValue("HTTP_DISPATCHER_ENDPOINT", "/api/handle-message"),
                            configuration.GetValue("HTTP_DISPATCHER_HEALTHCHECK", "/api/handle-message/is-alive")
                        )
                    )
                );
                break;
            default:
                throw new ArgumentException($"Unsupported dispatcher type {dispatcherType}");
        }
    }
}