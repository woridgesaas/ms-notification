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

public sealed class AbsenceAlertRhConsumer : BackgroundService
{
    private readonly IRabbitMQConnection _connection;
    private readonly RabbitMQSettings _settings;
    private readonly IServiceProvider _serviceProvider;
    private IChannel? _channel;

    public AbsenceAlertRhConsumer(
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
        queue: _settings.Queues.AbsenceAlertRh,
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
            queue: _settings.Queues.AbsenceAlertRh,
            durable: true,
            exclusive: false,
            autoDelete: false);

        await _channel.QueueBindAsync(
            queue: _settings.Queues.AbsenceAlertRh,
            exchange: _settings.Exchange,
            routingKey: _settings.RoutingKeys.AbsenceAlertRh);
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
            "AbsenceAlertRhConsumer — event null, message rejeté définitivement");
    }

    try
    {
        await ProcessEventAsync(eventData);
        await _channel!.BasicAckAsync(args.DeliveryTag, false);
    }
    catch (Exception ex)
    {
        await _channel!.BasicNackAsync(args.DeliveryTag, false, requeue: false);
        throw new AbsenceAlertConsumerException(ex.Message);
    }
}

private async Task ProcessEventAsync(AbsenceAlertEvent eventData)
{
    using var scope = _serviceProvider.CreateScope();
    var notificationService = scope.ServiceProvider
            .GetRequiredService<INotificationService>();

    // ── 1. Email vers l'employé absent ───────────────────────
    var emailRequest = new SendNotificationRequest<AbsenceAlertEvent>
    {
        Channel = Channel.EMAIL,
        TemplateCode = TemplateCode.ABSENCE_ALERT_USER,
        To = eventData.EmployeeEmail,
        CreatedBy = eventData.RecipientCins.FirstOrDefault()
                    ?? throw new AbsenceAlertConsumerException(
                        "AbsenceAlertRhConsumer — RecipientCins est vide"),
        Data = eventData
    };
    await notificationService.SendNotificationAsync(emailRequest);

    // ── 2. Push vers RH + Manager ─────────────────────────────
    foreach (var cin in eventData.RecipientCins)
    {
        var appRequest = new PushNotificationRequest
        {
            TargetCin = cin,
            TemplateName = "Attendance/ABSENCE_ALERT_RH",
            Channel = Channel.APP,
            NotificationType = NotificationType.ALERT,
            Priority = Priority.HIGHT,
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