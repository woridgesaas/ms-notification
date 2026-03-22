using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ms_notification.DTOs;
using ms_notification.Enum;
using ms_notification.Exceptions;
using ms_notification.Messaging.Events;
using ms_notification.Options;
using ms_notification.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ms_notification.Messaging.Consumers;

public sealed class AbsenceAlertCeoConsumer : BackgroundService
{
    private readonly IRabbitMQConnection _connection;
    private readonly RabbitMQSettings _settings;
    private readonly IServiceProvider _serviceProvider;
    private IChannel? _channel;

    public AbsenceAlertCeoConsumer(
        IRabbitMQConnection connection,
        IOptions<RabbitMQSettings> settings,
        IServiceProvider serviceProvider)
    {
        _connection = connection;
        _settings = settings.Value;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _channel = await _connection.CreateChannelAsync();
        await DeclareInfrastructureAsync();

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += HandleMessageAsync;

        await _channel.BasicConsumeAsync(
            queue: _settings.Queues.AbsenceAlertCeo,
            autoAck: false,
            consumer: consumer);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task DeclareInfrastructureAsync()
    {
        await _channel!.ExchangeDeclareAsync(
            exchange: _settings.Exchange,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false);

        await _channel.QueueDeclareAsync(
            queue: _settings.Queues.AbsenceAlertCeo,
            durable: true,
            exclusive: false,
            autoDelete: false);

        await _channel.QueueBindAsync(
            queue: _settings.Queues.AbsenceAlertCeo,
            exchange: _settings.Exchange,
            routingKey: _settings.RoutingKeys.AbsenceAlertCeo);
    }

    private async Task HandleMessageAsync(object _, BasicDeliverEventArgs args)
    {
        var json = Encoding.UTF8.GetString(args.Body.ToArray());

        var eventData = JsonSerializer.Deserialize<AbsenceAlertEvent>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (eventData is null)
        {
            await _channel!.BasicNackAsync(args.DeliveryTag, false, requeue: false);
            throw new AbsenceAlertConsumerException(
                "AbsenceAlertCeoConsumer — event null, message rejeté définitivement");
        }

        await ProcessEventAsync(eventData);
        await _channel!.BasicAckAsync(args.DeliveryTag, false);
    }

    private async Task ProcessEventAsync(AbsenceAlertEvent eventData)
    {
        using var scope = _serviceProvider.CreateScope();
        var notificationService = scope.ServiceProvider
                .GetRequiredService<INotificationService>();

        // ── Push vers CEO + Direction ─────────────────────────────
        foreach (var cin in eventData.RecipientCins)
        {
            var appRequest = new PushNotificationRequest
            {
                TargetCin = cin,
                TemplateName = "Attendance/ABSENCE_ALERT_CEO",
                Channel = Channel.APP,
                NotificationType = NotificationType.ALERT,
                Priority = Priority.CRITICAL,
                Data = new Dictionary<string, string>
            {
                { "EmployeeFullName", eventData.EmployeeFullName },
                { "EmployeeEmail",    eventData.EmployeeEmail    },
                { "AbsenceDate",      eventData.AbsenceDate      },
                { "ConsecutiveDays",  eventData.ConsecutiveDays.ToString() }
            }
            };
            await notificationService.SendPushNotificationAsync(appRequest);
        }
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