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
using System.Text.Json.Serialization;
using ms_notification.Enum;
using ms_notification.DTOs.Email;
namespace ms_notification.Messaging.Consumers
{
    public class LeaveHandleConsumer (IRabbitMQConnection _connection, IOptions<RabbitMQSettings> _settings , IServiceProvider _serviceProvider) : BackgroundService
    {
        private  IChannel _channel;
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _channel = await _connection.CreateChannelAsync();
            await this.PrepareInfrastructureAsync(); //Prepare exchange , declare queue and bind them together with a routing key

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                try
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    options.Converters.Add(new JsonStringEnumConverter());
                    var leaveAlert = JsonSerializer.Deserialize<LeaveHandleEvent>(message, options);
                    if (leaveAlert is null || String.IsNullOrEmpty(leaveAlert.EmployeeEmail) || String.IsNullOrEmpty(leaveAlert.EmployeeCin))
                    {
                        throw new RabbitMQConsumerException("Event null");
                    }
                    //Processing send
                    using var scope = _serviceProvider.CreateScope();
                    var _notificationService=scope.ServiceProvider.GetRequiredService <INotificationService>();
                    var factory = scope.ServiceProvider.GetRequiredService<INotificationRequestFactory>();
                    var request = factory.CreateRequest<LeaveProcessData>(leaveAlert , TemplateCode.LEAVE_APPROVE_ALERT);
                    await _notificationService.SendNotificationAsync<LeaveProcessData>(request);
                }
                catch (RabbitMQConsumerException)
                {
                    await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);

                }
                catch (Exception ex)
                {
                    await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);

                }
            };
            await _channel.BasicConsumeAsync(
               queue: _settings.Value.Queues.LeaveHandleAlert,
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
                queue: _settings.Value.Queues.LeaveHandleAlert, 
                durable: true, 
                exclusive: false, 
                autoDelete: false
                );

            await _channel.QueueBindAsync(
                queue: _settings.Value.Queues.LeaveHandleAlert,
                exchange: _settings.Value.Exchange, 
                routingKey: _settings.Value.RoutingKeys.LeaveProcessedAlert
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

    }
}
