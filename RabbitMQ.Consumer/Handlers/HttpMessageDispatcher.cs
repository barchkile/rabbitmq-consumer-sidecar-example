using System;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RabbitMQ.Consumer;

public class HttpMessageDispatcher : IMessageHandler, IDisposable
{
    private readonly HttpClient _client;
    private readonly HttpMessageDispatcherConfig _config;

    public HttpMessageDispatcher(HttpMessageDispatcherConfig config)
    {
        _config = config;
        _client = new HttpClient
        {
            BaseAddress = new Uri(config.BaseUrl),
            Timeout = Timeout.InfiniteTimeSpan
        };
    }

    public void Dispose()
        => _client.Dispose();

    public async Task<bool> HandleMessageAsync(QueueMessage queueMessage, int handlingTimeoutInSeconds, CancellationToken cancellationToken)
    {
        using var timeoutCancellationSource = new CancellationTokenSource();
        if (handlingTimeoutInSeconds > 0)
        {
            timeoutCancellationSource.CancelAfter(TimeSpan.FromSeconds(handlingTimeoutInSeconds));
        }

        using var combinedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCancellationSource.Token);

        var message = new HandlerQueueMessageRequestBody(queueMessage, handlingTimeoutInSeconds);
        var handleResponse = await PostMessageAsync(JsonConvert.SerializeObject(message), combinedCancellationTokenSource.Token);

        return handleResponse.Success;
    }

    public async Task<bool> IsReadyAsync()
    {
        try
        {
            var response = await _client.GetAsync(_config.HealthCheckEndpoint);
            response.EnsureSuccessStatusCode();

            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task<HandleMessageResponse> PostMessageAsync(string message, CancellationToken cancellationToken)
    {
        var response = await _client.PostAsync(
            _config.DispatchEndpoint,
            new StringContent(
                message,
                Encoding.UTF8,
                MediaTypeNames.Application.Json
            ),
            cancellationToken
        );

        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        return JsonConvert.DeserializeObject<HandleMessageResponse>(responseContent);
    }
}