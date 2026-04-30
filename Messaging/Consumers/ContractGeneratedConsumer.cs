using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ms_notification.Enum;
using ms_notification.Exceptions;
using ms_notification.Messaging.Events;
using ms_notification.Options;
using ms_notification.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ms_notification.Messaging.Consumers;

public sealed class ContractGeneratedConsumer : BackgroundService
{
    private readonly IRabbitMQConnection _connection;
    private readonly RabbitMQSettings    _settings;
    private readonly IServiceProvider    _serviceProvider;
    private IChannel? _channel;
    public ContractGeneratedConsumer(
        IRabbitMQConnection connection,
        IOptions<RabbitMQSettings> settings,
        IServiceProvider serviceProvider)
    {
        _connection      = connection;
        _settings        = settings.Value;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _channel = await _connection.CreateChannelAsync();

        await _channel.QueueDeclareAsync(
            queue:      _settings.Queues.ContractGenerated,
            durable:    true,
            exclusive:  false,
            autoDelete: false,
            arguments:  null);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += HandleMessageAsync;

        await _channel.BasicConsumeAsync(
            queue:    _settings.Queues.ContractGenerated,
            autoAck:  false,
            consumer: consumer);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

 private async Task HandleMessageAsync(object _, BasicDeliverEventArgs args)
{
    var json = Encoding.UTF8.GetString(args.Body.ToArray());
    try
    {
        var eventData = JsonSerializer.Deserialize<ContractGeneratedEvent>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (eventData is null)
        {
            await _channel!.BasicNackAsync(args.DeliveryTag, false, requeue: false);
            return;
        }

        await ProcessEventAsync(eventData);

        await _channel!.BasicAckAsync(args.DeliveryTag, false);
    }
    catch (Exception ex)
    {
        await _channel!.BasicNackAsync(args.DeliveryTag, false, requeue: false);
    }
}

    private async Task ProcessEventAsync(ContractGeneratedEvent eventData)
    {
        // Consumer BackgroundService = pas de contexte HTTP, pas de token
        // → CreatedBy = "SYSTEM" (EmailService skip la sauvegarde Notification dans ce cas)
        var request = new SendNotificationRequest<ContractGeneratedEvent>
        {
            Channel      = Channel.EMAIL,
            TemplateCode = TemplateCode.CONTRACT_GENERATED,
            To           = eventData.CandidateEmail,
            CreatedBy    = "SYSTEM",
            Data         = eventData
        };

        using var scope = _serviceProvider.CreateScope();
        var notificationService = scope.ServiceProvider
            .GetRequiredService<INotificationService>();

        await notificationService.SendNotificationAsync(request);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
        if (_channel is not null)
        {
            await _channel.CloseAsync();
            await _channel.DisposeAsync();
        }
    }
}