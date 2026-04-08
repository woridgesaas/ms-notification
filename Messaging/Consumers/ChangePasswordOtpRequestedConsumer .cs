using Microsoft.Extensions.Options;
using ms_notification.Enum;
using ms_notification.Messaging.Events;
using ms_notification.Options;
using ms_notification.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

public sealed class ChangePasswordOtpRequestedConsumer : BackgroundService
{
    private readonly IRabbitMQConnection _connection;
    private readonly RabbitMQSettings _settings;
    private readonly IServiceProvider _serviceProvider;
    private IChannel? _channel;

    public ChangePasswordOtpRequestedConsumer(
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
            queue: _settings.Queues.ChangePasswordOtp,
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
            queue: _settings.Queues.ChangePasswordOtp,
            durable: true,
            exclusive: false,
            autoDelete: false);

        await _channel.QueueBindAsync(
            queue: _settings.Queues.ChangePasswordOtp,
            exchange: _settings.Exchange,
            routingKey: _settings.RoutingKeys.ChangePasswordOtp);
    }

    private async Task HandleMessageAsync(object sender, BasicDeliverEventArgs args)
    {
        var json = Encoding.UTF8.GetString(args.Body.ToArray());
        await Task.Delay(5000);
        var data = JsonSerializer.Deserialize<ChangePasswordOtpRequestedEvent>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (data == null)
        {
            Console.WriteLine(" Message OTP invalide reçu");
            await _channel!.BasicNackAsync(args.DeliveryTag, false, false);
            return;
        }

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            var request = new SendNotificationRequest<ChangePasswordOtpRequestedEvent>
            {
                TemplateCode = TemplateCode.OTP,
                To = data.Email,
                CreatedBy = data.Cin,
                Data = data
            };

            await emailService.SendViaEmailAsync(request);

            Console.WriteLine($" OTP envoyé à {data.Email}");

            await _channel!.BasicAckAsync(args.DeliveryTag, false);
        }
        catch (Exception ex)
        {
            Console.WriteLine($" Erreur envoi OTP: {ex.Message}");
            await _channel!.BasicNackAsync(args.DeliveryTag, false, false);
        }
    }
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);

        if (_channel != null)
        {
            await _channel.CloseAsync();
            await _channel.DisposeAsync();
        }
    }
}