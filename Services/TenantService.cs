using Microsoft.Data.SqlClient;

namespace ms_notification.Services
{
    public class TenantService :ITenantService
    {
        private IConfiguration _configuration;
        private ILogger<TenantService> _logger;
        public TenantService(IConfiguration configuration, ILogger<TenantService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }
        public string Subdomain { get; set; } = null;

        public string ServiceKey { get => "ms-notif"; }

        public string GetConnectionString()
        {
            var baseConfig = this._configuration.GetConnectionString("DefaultConnection");
            SqlConnectionStringBuilder sqlStringBuilder = new(baseConfig);
            if (string.IsNullOrEmpty(Subdomain) || string.IsNullOrEmpty(ServiceKey))
            {
                _logger.LogError("[TenantService]: Tenant subdomain or service key is null or empty. Returning base connection string.");
                return baseConfig;
            }
            sqlStringBuilder.InitialCatalog = $"woridge_{Subdomain}_{ServiceKey}";
            return sqlStringBuilder.ConnectionString;
        }
    }
}

