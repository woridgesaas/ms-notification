using Microsoft.Extensions.Options;
using ms_notification.Data;
using ms_notification.Messaging.Events;
using ms_notification.Models;
using ms_notification.Options;
using ms_notification.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace ms_notification.Messaging.Consumers
{
    public sealed class UserSyncConsumer : BackgroundService
    {
        private readonly IRabbitMQConnection _connection;
        private readonly RabbitMQSettings _settings;
        private readonly IServiceProvider _serviceProvider;
        private IChannel? _channel;

        public UserSyncConsumer(
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
                queue: _settings.Queues.UserSyncCreated,
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
                queue: _settings.Queues.UserSyncCreated,
                durable: true,
                exclusive: false,
                autoDelete: false);

            await _channel.QueueBindAsync(
                queue: _settings.Queues.UserSyncCreated,
                exchange: _settings.Exchange,
                routingKey: _settings.RoutingKeys.UserSyncCreated);
        }

        private async Task HandleMessageAsync(object _, BasicDeliverEventArgs args)
        {
            var json = Encoding.UTF8.GetString(args.Body.ToArray());

            try
            {
                var eventData = JsonSerializer.Deserialize<UserCreatedEvent>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (eventData == null)
                {
                    await _channel!.BasicNackAsync(args.DeliveryTag, false, false);
                    return;
                }
                if (!json.Contains("Cin") && !json.Contains("cin"))
                {
                    Console.WriteLine(" Not a UserSync event → ignored");
                    await _channel!.BasicAckAsync(args.DeliveryTag, false);
                    return;
                }
                await ProcessEventAsync(eventData);

                await _channel!.BasicAckAsync(args.DeliveryTag, false);
            }
            catch
            {
                await _channel!.BasicNackAsync(args.DeliveryTag, false, false);
            }
        }

        private async Task ProcessEventAsync(UserCreatedEvent eventData)
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();

            var existingUser = db.Users.FirstOrDefault(u => u.CIN == eventData.Cin);

            if (existingUser == null)
            {
                var user = new User
                {
                    CIN = eventData.Cin,
                    FirstName = eventData.FirstName,
                    LastName = eventData.LastName,
                    Email = eventData.Email,
                    Role = eventData.Role 
                };

                db.Users.Add(user);
            }
            else
            {
                existingUser.FirstName = eventData.FirstName;
                existingUser.LastName = eventData.LastName;
                existingUser.Email = eventData.Email;
                existingUser.Role = eventData.Role; 
            }

            await db.SaveChangesAsync();
        }
    }
}
