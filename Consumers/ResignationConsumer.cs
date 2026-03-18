using ms_notification.DTOs.Events;
using ms_notification.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ms_notification.Consumers;

public class ResignationConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ResignationConsumer> _logger;
    private readonly IConfiguration _configuration;

    public ResignationConsumer(
        IServiceScopeFactory scopeFactory,
        ILogger<ResignationConsumer> logger,
        IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("ResignationConsumer — tentative de connexion à RabbitMQ...");

            var host = _configuration["RabbitMQ:Host"];
            var port = _configuration["RabbitMQ:Port"];
            var user = _configuration["RabbitMQ:Username"];
            var pass = _configuration["RabbitMQ:Password"];

            _logger.LogInformation(
                "Config RabbitMQ → Host: {Host}, Port: {Port}, User: {User}",
                host, port, user);

            var factory = new ConnectionFactory
            {
                HostName = host!,
                Port = int.Parse(port!),
                UserName = user!,
                Password = pass!
            };

            var connection = await factory.CreateConnectionAsync(stoppingToken);
            var channel = await connection.CreateChannelAsync(
                cancellationToken: stoppingToken);

            await channel.QueueDeclareAsync(
                queue: "resignation.notification.queue",
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: stoppingToken
            );

            await channel.BasicQosAsync(
                prefetchSize: 0,
                prefetchCount: 1,
                global: false,
                cancellationToken: stoppingToken
            );

            _logger.LogInformation("ResignationConsumer démarré — écoute la queue");

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (_, args) =>
            {
                try
                {
                    var json = Encoding.UTF8.GetString(args.Body.ToArray());
                    _logger.LogInformation("JSON brut reçu : {Json}", json);

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        Converters = { new JsonStringEnumConverter() }
                    };

                    var evt = JsonSerializer.Deserialize<ResignationCreatedEvent>(
                        json, options);

                    if (evt is null)
                    {
                        _logger.LogWarning("Message vide — ignoré");
                        await channel.BasicAckAsync(args.DeliveryTag, multiple: false);
                        return;
                    }

                    using var scope = _scopeFactory.CreateScope();
                    var notifService = scope.ServiceProvider
                        .GetRequiredService<INotificationService>();

                    await notifService.SendResignationNotificationAsync(evt);

                    await channel.BasicAckAsync(args.DeliveryTag, multiple: false);
                    _logger.LogInformation(
                        "Email résignation envoyé → {Email}", evt.EmployeeEmail);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur traitement message : {Message}", ex.Message);
                    await channel.BasicNackAsync(
                        args.DeliveryTag, multiple: false, requeue: false);
                }
            };

            await channel.BasicConsumeAsync(
                queue: "resignation.notification.queue",
                autoAck: false,
                consumer: consumer
            );

            // Attendre jusqu'à l'arrêt de l'application
            await Task.Delay(Timeout.Infinite, stoppingToken);

            // Nettoyage propre
            await channel.CloseAsync();
            await connection.CloseAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ERREUR CONSUMER au démarrage : {Message}", ex.Message);
        }
    }
}