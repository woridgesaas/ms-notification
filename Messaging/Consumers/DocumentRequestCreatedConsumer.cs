using Microsoft.Extensions.Options;
using ms_notification.Data;
using ms_notification.DTOs;
using ms_notification.Enum;
using ms_notification.Messaging.Events;
using ms_notification.Options;
using ms_notification.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace ms_notification.Messaging.Consumers
{
    public sealed class DocumentRequestCreatedConsumer : BackgroundService
    {
        private readonly IRabbitMQConnection _connection;
        private readonly RabbitMQSettings _settings;
        private readonly IServiceProvider _serviceProvider;
        private IChannel? _channel;

        public DocumentRequestCreatedConsumer(
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
                queue: _settings.Queues.DocumentRequestCreated,
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
                queue: _settings.Queues.DocumentRequestCreated,
                durable: true,
                exclusive: false,
                autoDelete: false);

            await _channel.QueueBindAsync(
                queue: _settings.Queues.DocumentRequestCreated,
                exchange: _settings.Exchange,
                routingKey: _settings.RoutingKeys.DocumentRequestCreated);
        }

        private async Task HandleMessageAsync(object _, BasicDeliverEventArgs args)
        {
            var json = Encoding.UTF8.GetString(args.Body.ToArray());

            Console.WriteLine("🔥 DOC EVENT: " + json);

            try
            {
                var eventData = JsonSerializer.Deserialize<DocumentRequestCreatedEvent>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (eventData == null)
                {
                    await _channel!.BasicAckAsync(args.DeliveryTag, false);
                    return;
                }

                await ProcessEventAsync(eventData);

                await _channel!.BasicAckAsync(args.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ ERROR: " + ex.Message);
                Console.WriteLine("❌ STACK: " + ex.StackTrace);

                await _channel!.BasicNackAsync(args.DeliveryTag, false, false);
            }
        }

        private async Task ProcessEventAsync(DocumentRequestCreatedEvent eventData)
        {
            using var scope = _serviceProvider.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            var rhUsers = db.Users
                .Where(u => u.Role == "HR")
                .ToList();

            foreach (var rh in rhUsers)
            {
                Console.WriteLine("Sending to RH: " + rh.CIN);
                Console.WriteLine("Employee CIN = " + eventData.EmployeeCin);

                var request = new SendNotificationRequest<object>
                {
                    Channel = Channel.APP,
                    TemplateCode = TemplateCode.DOCUMENT_REQUEST_CREATED,
                    To = rh.CIN,
                    CreatedBy = eventData.EmployeeCin,
                    Data = new Dictionary<string, string> {
    { "employeeName", eventData.EmployeeFullName },   // FIX
    { "employeeCin", eventData.EmployeeCin },
    { "documentCode", eventData.DocumentCode },       // AJOUT
    { "documentGroup", eventData.DocumentGroup }      // FIX
}
                };

                Console.WriteLine("TEMPLATE NAME = " + request.TemplateCode);

                await notificationService.SendNotificationAsync(request);
            }
        }
    }
}
