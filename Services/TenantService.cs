using Microsoft.Data.SqlClient;

namespace ms_notification.Services
{
    public class TenantService :ITenantService
    {
        private IConfiguration _configuration;
        private ILogger<TenantService> _logger;

        private const string DEFAULT_SUBDOMAIN = "dev-local";
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
            if (string.IsNullOrEmpty(Subdomain) || Subdomain == DEFAULT_SUBDOMAIN)
            {
                _logger.LogInformation("[TenantService] Using default connection string for subdomain: {Subdomain}", Subdomain);
                return baseConfig; 
            }
            sqlStringBuilder.InitialCatalog = $"woridge_{Subdomain}_{ServiceKey}";
            return sqlStringBuilder.ConnectionString;
        }
    }
}

