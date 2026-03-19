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

public sealed class ResignationConsumer : BackgroundService
{
    private readonly IRabbitMQConnection _connection;
    private readonly RabbitMQSettings _settings;
    private readonly IServiceProvider _serviceProvider;
    private IChannel? _channel;

    public ResignationConsumer(
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
            queue: _settings.Queues.ResignationCreated,
            autoAck: false,
            consumer: consumer
        );

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
            queue: _settings.Queues.ResignationCreated,
            durable: true,
            exclusive: false,
            autoDelete: false
        );

        await _channel.QueueBindAsync(
            queue: _settings.Queues.ResignationCreated,
            exchange: _settings.Exchange,
            routingKey: _settings.RoutingKeys.ResignationCreated
        );
    }

    private async Task HandleMessageAsync(object _, BasicDeliverEventArgs args)
    {
        var json = Encoding.UTF8.GetString(args.Body.ToArray());

        try
        {
            var eventData = JsonSerializer.Deserialize<ResignationCreatedEvent>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (eventData is null)
            {
                await _channel!.BasicNackAsync(args.DeliveryTag, false, requeue: false);
                throw new RabbitMQConsumerException("Event null — rejeté");
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
            await _channel!.BasicNackAsync(args.DeliveryTag, false, requeue: false);
            throw new RabbitMQConsumerException(ex.Message);
        }
    }

    private async Task ProcessEventAsync(ResignationCreatedEvent eventData)
    {
        var request = new SendNotificationRequest<ResignationData>
        {
            Channel = Channel.EMAIL,
            TemplateCode = TemplateCode.RESIGNATION_CONFIRMATION,
            To = eventData.EmployeeEmail,
            CreatedBy = eventData.EmployeeCin,
            Data = new ResignationData
            {
                EmployeeFullName = eventData.EmployeeFullName,
                ResignationDate = eventData.ResignationDate.ToString("dd/MM/yyyy"),
                Reason = eventData.Reason,
                CompanyName = eventData.CompanyName
            }
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