namespace ms_notification.Options;

public sealed class RabbitMQSettings
{
    public string Host     { get; set; } = default!;
    public int    Port     { get; set; }
    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string Exchange { get; set; } = default!;

    public RabbitMQQueues      Queues      { get; set; } = new();
    public RabbitMQRoutingKeys RoutingKeys { get; set; } = new();
}