using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;
using ms_notification.DTOs;
using ms_notification.DTOs.Email;
using ms_notification.Enum;
using ms_notification.Exceptions;
using ms_notification.Messaging.Events;
using ms_notification.Options;
using ms_notification.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ms_notification.Messaging.Consumers;

public sealed class InterviewCreatedConsumer : BackgroundService
{
    private readonly IRabbitMQConnection _connection;
    private readonly RabbitMQSettings _settings;
    private readonly IServiceProvider _serviceProvider;
    private IChannel? _channel;

    public InterviewCreatedConsumer(
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
            queue: _settings.Queues.InterviewCreated,
            autoAck: false,
            consumer: consumer
        );

        Console.WriteLine($"InterviewCreatedConsumer démarré — écoute sur {_settings.Queues.InterviewCreated}");

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task DeclareInfrastructureAsync()
    {
        await _channel!.ExchangeDeclareAsync(
            exchange: _settings.Exchange,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false
        );

        await _channel.QueueDeclareAsync(
            queue: _settings.Queues.InterviewCreated,
            durable: true,
            exclusive: false,
            autoDelete: false
        );

        await _channel.QueueBindAsync(
            queue: _settings.Queues.InterviewCreated,
            exchange: _settings.Exchange,
            routingKey: _settings.RoutingKeys.InterviewCreated
        );
    }

    private async Task HandleMessageAsync(object _, BasicDeliverEventArgs args)
    {
        var json = Encoding.UTF8.GetString(args.Body.ToArray());

        Console.WriteLine($"=== Message brut reçu ===");
        Console.WriteLine(json);
        Console.WriteLine("=========================");

        try
        {
            var eventData = JsonSerializer.Deserialize<InterviewCreatedEvent>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (eventData is null)
            {
                Console.WriteLine("Event null — ignoré");
                await _channel!.BasicNackAsync(args.DeliveryTag, false, requeue: false);
                return;
            }

            await ProcessEventAsync(eventData);
            await _channel!.BasicAckAsync(args.DeliveryTag, false);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERREUR HandleMessageAsync: {ex.Message}");
            await _channel!.BasicNackAsync(args.DeliveryTag, false, requeue: false);
            throw new RabbitMQConsumerException(ex.Message);
        }
    }

    private async Task ProcessEventAsync(InterviewCreatedEvent eventData)
    {
        Console.WriteLine("=== InterviewCreatedConsumer ProcessEventAsync ===");
        Console.WriteLine($"CandidateName:   '{eventData.CandidateName}'");
        Console.WriteLine($"CandidateEmail:  '{eventData.CandidateEmail}'");
        Console.WriteLine($"InterviewerName: '{eventData.InterviewerName}'");
        Console.WriteLine($"InterviewerEmail:'{eventData.InterviewerEmail}'");
        Console.WriteLine($"InterviewerCin:  '{eventData.InterviewerCin}'");
        Console.WriteLine($"StartDateTime:   '{eventData.StartDateTime}'");
        Console.WriteLine($"Type:            '{eventData.Type}'");
        Console.WriteLine("==================================================");

        using var scope = _serviceProvider.CreateScope();
        var notificationService = scope.ServiceProvider
            .GetRequiredService<INotificationService>();

        var startDate = DateTime.Parse(eventData.StartDateTime)
                                .ToString("dd/MM/yyyy à HH:mm");

        // Email candidat
        Console.WriteLine($"Envoi email candidat → '{eventData.CandidateEmail}'");
        await notificationService.SendNotificationAsync(
            new SendNotificationRequest<InterviewScheduledData>
            {
                Channel = Channel.EMAIL,
                TemplateCode = TemplateCode.INTERVIEW_SCHEDULED,
                To = eventData.CandidateEmail,
                CreatedBy = "SYSTEM",
                Data = new InterviewScheduledData
                {
                    CandidateName = eventData.CandidateName,
                    InterviewerName = eventData.InterviewerName,
                    StartDateTime = startDate,
                    DurationMinutes = eventData.DurationMinutes.ToString(),
                    Type = eventData.Type,
                    VideoConferenceLink = eventData.VideoConferenceLink,
                    Location = eventData.Location,
                    PhoneNumber = eventData.PhoneNumber,
                    TargetPosition = eventData.TargetPosition,
                    CompanyName = "Woridge"
                }
            });
        Console.WriteLine($"Email candidat envoyé ✅");

        // Email interviewer
        Console.WriteLine($"Envoi email interviewer → '{eventData.InterviewerEmail}'");
        await notificationService.SendNotificationAsync(
            new SendNotificationRequest<InterviewAssignedData>
            {
                Channel = Channel.EMAIL,
                TemplateCode = TemplateCode.INTERVIEW_ASSIGNED,
                To = eventData.InterviewerEmail,
                CreatedBy = "SYSTEM",
                Data = new InterviewAssignedData
                {
                    InterviewerName = eventData.InterviewerName,
                    CandidateName = eventData.CandidateName,
                    StartDateTime = startDate,
                    DurationMinutes = eventData.DurationMinutes.ToString(),
                    Type = eventData.Type,
                    VideoConferenceLink = eventData.VideoConferenceLink,
                    Location = eventData.Location,
                    PhoneNumber = eventData.PhoneNumber,
                    TargetPosition = eventData.TargetPosition,
                    CompanyName = "Woridge"
                }
            });
        Console.WriteLine($"Email interviewer envoyé ");
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