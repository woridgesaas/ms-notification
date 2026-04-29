using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ms_notification.Data;

namespace ms_notification.Factories
{
    public class TenantConnectionFactory
    {
        public IConfiguration _configuration { get; set; }
        public TenantConnectionFactory(IConfiguration configuration)
        {
            this._configuration = configuration;
        }
        public NotificationDbContext CreateDbContextForTenant(string subdomain, string serviceKey)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString);

            DbContextOptionsBuilder<NotificationDbContext> options = new DbContextOptionsBuilder<NotificationDbContext>();
            options.UseSqlServer(builder.ConnectionString);
            builder.InitialCatalog = $"woridge_{subdomain}_{serviceKey}";
            options.UseSqlServer(builder.ConnectionString);
            return new NotificationDbContext(options.Options);
        }
    }
}

