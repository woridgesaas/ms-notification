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


public sealed class CreateUserConsumer : BackgroundService//pour tourner en arrière-plan
{
    private ILogger<CreateUserConsumer> _logger;
    private readonly IRabbitMQConnection _connection;
    private readonly RabbitMQSettings _settings;
    private readonly IServiceProvider _serviceProvider;
    private IChannel? _channel;

    public CreateUserConsumer(
        IRabbitMQConnection connection,
        IOptions<RabbitMQSettings> settings,
        IServiceProvider serviceProvider,
        ILogger<CreateUserConsumer> logger
        )
    {
        _connection = connection;
        _settings = settings.Value;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _channel = await _connection.CreateChannelAsync();

        await DeclareInfrastructureAsync();

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += HandleMessageAsync;

        await _channel.BasicConsumeAsync(
            queue: _settings.Queues.UserCreated,
            autoAck: false,
            consumer: consumer);

        _logger.LogDebug("Consuming on exchnage {}" , _settings.Exchange);
        // Maintient le service actif jusqu'à l'arrêt de l'application
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
            queue: _settings.Queues.UserCreated,
            durable: true,
            exclusive: false,
            autoDelete: false);

        await _channel.QueueBindAsync(
            queue: _settings.Queues.UserCreated,
            exchange: _settings.Exchange,
            routingKey: _settings.RoutingKeys.UserCreated);
    }


    private async Task HandleMessageAsync(object _, BasicDeliverEventArgs args)
    {
        var json = Encoding.UTF8.GetString(args.Body.ToArray());
        _logger.LogDebug("Raw JSON received : {}" , json);
        try
        {
            var eventData = JsonSerializer.Deserialize<CreateUserNotificationEvent>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (eventData is null)
            {
                await _channel!.BasicNackAsync(args.DeliveryTag, false, requeue: false);
                throw new RabbitMQConsumerException("Event null — message rejeté définitivement");
            }
            await ProcessEventAsync(eventData);

            await _channel!.BasicAckAsync(args.DeliveryTag, false);
        }
        catch (RabbitMQConsumerException)
        {
            await _channel!.BasicNackAsync(args.DeliveryTag, false, requeue: false);
        }
        catch (Exception ex)
        {
            // requeue: false — évite la boucle infinie
            await _channel!.BasicNackAsync(args.DeliveryTag, false, requeue: false);
            throw new RabbitMQConsumerException(ex.Message);
        }
    }

    private async Task ProcessEventAsync(CreateUserNotificationEvent eventData)
    {
        var request = new SendNotificationRequest<CreateUserNotificationEvent>
        {
            Channel = Channel.EMAIL,
            TemplateCode = TemplateCode.USER_CREATED,
            To = eventData.Email,
            CreatedBy = "S760251",//future: remplacer par un cin qui vient du api gateway
            Data = eventData
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