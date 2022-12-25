using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMQ.Consumer;

public class QueueConsumer : IQueueConsumer
{
    private readonly IRabbitMQConnectionProvider _connectionProvider;
    private readonly QueueConsumerConfig _consumerConfig;

    private readonly TimeSpan _handlerReadinessCheckInterval = TimeSpan.FromSeconds(2);
    private readonly IMessageHandler _messageHandler;
    private readonly QueueConfig _queueConfig;

    private readonly ConcurrentDictionary<string, Task> _runningConsumes = new();
    private IModel _channel;
    private IConnection _connection;
    private EventingBasicConsumer _consumer;

    public QueueConsumer(
        string queueName,
        QueueConsumerConfig queueConsumerConfig,
        QueuesDefinitions queuesDefinitions,
        IRabbitMQConnectionProvider connectionProvider,
        IMessageHandler messageHandler
    )
    {
        _queueConfig = queuesDefinitions.Queues.GetValueOrDefault(queueName);
        _connectionProvider = connectionProvider;
        QueueName = queueName;
        _consumerConfig = queueConsumerConfig;
        _messageHandler = messageHandler;
    }
    
    public string QueueName { get; }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await WaitForHandlerAsync(_messageHandler, cancellationToken);
        }
        catch (Exception exception) when (cancellationToken.IsCancellationRequested || exception is TaskCanceledException)
        {
            return;
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }
        
        Connect();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Stopping...");
        StopConsume();
        DisposeChannel();
        DisposeConnection();

        try
        {
            await Task.WhenAll(_runningConsumes.Values)
                .WaitAsync(cancellationToken);
        }
        catch
        {
            // ignored
        }
        
        Console.WriteLine("Stopping...");
    }

    private void StartConsume()
    {
        _consumer = new EventingBasicConsumer(_channel);
        _consumer.Received += ConsumerOnReceived;

        _channel.BasicConsume(
            QueueName,
            false,
            _consumer
        );
    }

    private void StopConsume()
    {
        if (_consumer == null)
        {
            return;
        }

        try
        {
            _consumer.Received -= ConsumerOnReceived;
        }
        catch
        {
            // ignored
        }
        finally
        {
            _consumer = null;
        }
    }

    private void ConsumerOnReceived(object sender, BasicDeliverEventArgs eventArgs)
    {
        var message = QueueMessage.FromBasicProperties(
            eventArgs.BasicProperties,
            eventArgs.Body,
            QueueName
        );
        
        CreateMessageHandlingTask(((EventingBasicConsumer)sender).Model, message, eventArgs.DeliveryTag);
    }

    private void Connect()
    {
        Console.WriteLine("Connecting...");
        InitConnection();
        InitChannel();
        StartConsume();
        Console.WriteLine($"Connected and now consuming from queue {QueueName}");
    }

    private void InitChannel()
    {
        _channel = _connection.CreateModel();
        _channel.DeclareQueueByQueueConfig(QueueName, _queueConfig);
        _channel.BasicQos(
            0,
            _consumerConfig.MaxConcurrentConsumes,
            false
        );
    }

    private void InitConnection()
        => _connection ??= _connectionProvider.GetConnection($"{nameof(QueueConsumer)}_{QueueName}");

    private void DisposeConnection()
    {
        try
        {
            _connection?.Dispose();
        }
        catch
        {
            // ignored
        }
        finally
        {
            _connection = null;
        }
    }

    private void DisposeChannel()
    {
        try
        {
            _channel?.Dispose();
        }
        catch
        {
            // ignored
        }
        finally
        {
            _channel = null;
        }
    }
    
    private async Task WaitForHandlerAsync(IMessageHandler messageHandler, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && !await messageHandler.IsReadyAsync())
        {
            await Task.Delay(_handlerReadinessCheckInterval, cancellationToken);
        }

        cancellationToken.ThrowIfCancellationRequested();
    }

    private void CreateMessageHandlingTask(
        IModel channel,
        QueueMessage message,
        ulong deliveryTag
    )
    {
        var taskId = Guid.NewGuid()
            .ToString();

        Console.WriteLine($"Start handling message with taskId {taskId}, message: {message}");
        _runningConsumes[taskId] = HandleMessageAsync(channel, message, deliveryTag)
            .ContinueWith(t =>
                {
                    Console.WriteLine($"Done handling message with taskId {taskId}");
                    return _runningConsumes.TryRemove(taskId, out _);
                }
            );
    }

    private async Task HandleMessageAsync(IModel channel, QueueMessage message, ulong deliveryTag)
    {
        try
        {
            if (await _messageHandler.HandleMessageAsync(
                    message,
                    _consumerConfig.HandlingTimeoutInSeconds,
                    CancellationToken.None
                ))
            {
                SendAck(channel, deliveryTag);
            }
            else
            {
                SendNack(channel, deliveryTag, false);
            }
        }
        catch
        {
            SendNack(channel, deliveryTag, true);
        }
    }

    private static void SendNack(IModel channel, ulong deliveryTag, bool shouldRequeue)
    {
        Console.WriteLine("Sending nack");
        try
        {
            if (!channel.IsOpen)
            {
                return;
            }

            channel.BasicNack(deliveryTag, false, shouldRequeue);
        }
        catch
        {
            // ignored
        }
    }

    private void SendAck(IModel channel, ulong deliveryTag)
    {
        Console.WriteLine("Sending ack");
        try
        {
            if (!channel.IsOpen)
            {
                return;
            }
            
            channel.BasicAck(deliveryTag, false);
        }
        catch
        {
            // ignored
        }
    }
}