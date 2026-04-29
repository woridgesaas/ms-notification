using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ms_notification.Exceptions;
using ms_notification.Factories;
using ms_notification.Messaging.Events;
using ms_notification.Options;
using ms_notification.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace ms_notification.Messaging.Consumers
{
    public class TenantReadyConsumer : BackgroundService
    {
        private ILogger<TenantReadyConsumer> _logger;
        private readonly IRabbitMQConnection _connection;
        private readonly IOptions<RabbitMQSettings> _settings;
        private IChannel _channel = null;
        private IServiceProvider _serviceProvider;
        public TenantReadyConsumer(
            ILogger<TenantReadyConsumer> logger,
           IRabbitMQConnection connection,
           IOptions<RabbitMQSettings> rabbitSettings,
            IServiceProvider serviceProvider
            )
        {
            _logger = logger;
            _connection = connection;
            _settings = rabbitSettings;
            _serviceProvider = serviceProvider;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _channel = await _connection.CreateChannelAsync();
            await this.PrepareInfrastructureAsync();

            _logger.LogInformation("RabbitMQ consumer started for queue: {Queue}", _settings.Value.Queues.TenantReady);

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (sender, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine($"RAW: {message}");
                    var optionsSerializer = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    var tenantEvent = JsonSerializer.Deserialize<TenantReadyEvent>(message, optionsSerializer);
                    if (tenantEvent is null || String.IsNullOrEmpty(tenantEvent.Subdomain) || String.IsNullOrEmpty(tenantEvent.ServiceKey))
                    {
                        _logger.LogWarning("Invalid message received. RoutingKey: {RoutingKey}", ea.RoutingKey);
                        throw new RabbitMQConsumerException("Event null");
                    }

                    _logger.LogDebug("Message received: {Message}", message);
                    await ProcessTenantReadyAsync(tenantEvent);


                    await _channel.BasicAckAsync(ea.DeliveryTag, false);
                    _logger.LogDebug("Message processed successfully: {DeliveryTag}", ea.DeliveryTag);

                }
                catch (RabbitMQConsumerException ex)
                {
                    _logger.LogError(ex, "Consumer error - message rejected : {ExceptionMessage}", ex.Message);
                    await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "Unexpected error in consumer");
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
                }
            };

            await _channel.BasicConsumeAsync(
              queue: _settings.Value.Queues.TenantReady,
              autoAck: false,
              consumer: consumer,
              cancellationToken: stoppingToken
          );
        }

        private async Task PrepareInfrastructureAsync()
        {
            await _channel.ExchangeDeclareAsync(
                exchange: _settings.Value.Exchange,
                type: ExchangeType.Topic,
                durable: true
                );

            await _channel.QueueDeclareAsync(
                queue: _settings.Value.Queues.TenantReady,
                durable: true,
                exclusive: false,
                autoDelete: false
                );

            await _channel.QueueBindAsync(
                queue: _settings.Value.Queues.TenantReady,
                exchange: _settings.Value.Exchange,
                routingKey: _settings.Value.RoutingKeys.TenantReady
                );
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

        public async Task ProcessTenantReadyAsync(TenantReadyEvent tenantEvent)
        {

            var scope = _serviceProvider.CreateScope();

            var _tenantConnetionFactory = scope.ServiceProvider.GetRequiredService<TenantConnectionFactory>();

            using var context = _tenantConnetionFactory.CreateDbContextForTenant(tenantEvent.Subdomain, tenantEvent.ServiceKey);
            this._logger.LogInformation("Processing TenantReadyEvent: SubscriptionId {SubscriptionId}, Subdomain {Subdomain}, ServiceKey {ServiceKey}",
                            tenantEvent.SubscriptionId, tenantEvent.Subdomain, tenantEvent.ServiceKey);

            await context.Database.MigrateAsync();
            this._logger.LogInformation("Migration executed successfully for tenant : {Subdomain}", tenantEvent.Subdomain);

        }
    }
}
