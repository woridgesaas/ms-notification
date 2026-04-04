using Microsoft.Extensions.Options;
using ms_notification.Exceptions;
using ms_notification.Messaging.Events;
using ms_notification.Options;
using ms_notification.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ms_notification.DTOs.Email;
using ms_notification.Factories;

namespace ms_notification.Messaging.Consumers
{
    public class OvertimeHandleConsumer : BackgroundService
    {
        private readonly IRabbitMQConnection _connection;
        private readonly IOptions<RabbitMQSettings> _settings;
        private readonly IServiceProvider _serviceProvider;
        private IChannel? _channel;
        public OvertimeHandleConsumer(IRabbitMQConnection connection,IOptions<RabbitMQSettings> settings,IServiceProvider serviceProvider)
        {
            _connection = connection;
            _settings = settings;
            _serviceProvider = serviceProvider;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            
            _channel = await _connection.CreateChannelAsync(); 
            await this.PrepareInfrastructureAsync(); //Prepare exchange , declare queue and bind them together with a routing key

            var consumer = new AsyncEventingBasicConsumer(_channel);

            //subscribing to the Received event of the consumer
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                try
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    options.Converters.Add(new JsonStringEnumConverter());
                    var overtimeAlert = JsonSerializer.Deserialize<OvertimeHandleEvent>(message, options);

                    if (overtimeAlert is null || String.IsNullOrEmpty(overtimeAlert.EmployeeEmail) || String.IsNullOrEmpty(overtimeAlert.EmployeeCin))
                    {

                        throw new RabbitMQConsumerException("Event null");

                    }

                    //Processing send
                    using var scope = this._serviceProvider.CreateScope();
                    var _notificationService=scope.ServiceProvider.GetRequiredService <INotificationService>();
                    var factory = scope.ServiceProvider.GetRequiredService<INotificationRequestFactory>();

                    var request = factory.CreateOvertimeRequest(overtimeAlert);
                    await _notificationService.SendNotificationAsync<OvertimeProcessData>(request);

                    await _channel.BasicAckAsync(ea.DeliveryTag, false);

                }catch(RabbitMQConsumerException ex)
                {
                    await this._channel.BasicNackAsync(ea.DeliveryTag, multiple:false , requeue:false);

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[FATAL] Erreur de traitement : {ex.Message}");
                    if (ex.InnerException != null)
                        Console.WriteLine($"[INNER] : {ex.InnerException.Message}");
                    await this._channel.BasicNackAsync(ea.DeliveryTag, multiple:false , requeue:false);
                }
                
            };

            await _channel.BasicConsumeAsync(
                queue: this._settings.Value.Queues.OvertimeHandleAlert,
                autoAck: false,
                consumer: consumer,
                cancellationToken: stoppingToken
            );
        }
        private async Task PrepareInfrastructureAsync()
        {
            await this._channel!.ExchangeDeclareAsync(
                this._settings.Value.Exchange,
                type: ExchangeType.Topic,
                durable: true
                );
            await this._channel!.QueueDeclareAsync(
                queue: this._settings.Value.Queues.OvertimeHandleAlert,
                durable: true,
                exclusive: false,
                autoDelete: false);
            await this._channel!.QueueBindAsync(
                queue: this._settings.Value.Queues.OvertimeHandleAlert,
                exchange: this._settings.Value.Exchange,
                routingKey: this._settings.Value.RoutingKeys.OvertimeProcessedAlert
                );
        }
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            
            await base.StopAsync(cancellationToken);

            
            if (_channel is not null)
            {
                await _channel.CloseAsync(); // close canal local
                await _channel.DisposeAsync(); // clean RAM
            }
        }
    }
}
