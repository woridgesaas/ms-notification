using ms_notification.Services;

namespace ms_notification.Middleware
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;
            private readonly ILogger<TenantMiddleware> _logger;
        public TenantMiddleware(RequestDelegate requestDelegate , ILogger<TenantMiddleware> logger) {
            _next = requestDelegate; 
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, ITenantService tenantService)
        {
            if (context.Request.Headers.TryGetValue("X-Subdomain", out var subdomain))
            {
                tenantService.Subdomain = subdomain;
                _logger.LogInformation("[TenantMiddleware]: Tenant subdomain set to: {Subdomain}", subdomain);
            }
            await _next(context);
        }
    }
}
