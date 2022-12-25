namespace RabbitMQ.Consumer;

public record HttpMessageDispatcherConfig(string BaseUrl, string DispatchEndpoint, string HealthCheckEndpoint);