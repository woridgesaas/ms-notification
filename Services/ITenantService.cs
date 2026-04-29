namespace ms_notification.Services
{
    public interface ITenantService
    {
        string Subdomain { get; set; }
        string ServiceKey { get; }

        string GetConnectionString();
    }
}
