using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace ms_notification.Middleware
{
    public class HeaderAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private ILogger<HeaderAuthenticationHandler> _logger;
        public HeaderAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder ) : base(options, logger, encoder )
        {
            this._logger = logger.CreateLogger<HeaderAuthenticationHandler>();
        }


        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            string cinClaim=string.Empty , tenantId = string.Empty , roleClaim = string.Empty;
            if (Request.Headers.TryGetValue("X-User-CIN", out var cin)
                && Request.Headers.TryGetValue("X-User-Role", out var role)
                && Request.Headers.TryGetValue("X-Subdomain" , out var subdomain)
                )
            {
                cinClaim=cin.ToString();
                tenantId=subdomain.ToString();
                roleClaim=role.ToString();
            }
            else
            {
                this._logger.LogWarning("Authentication failed: Missing required headers.");
                return Task.FromResult(AuthenticateResult.Fail("Missing Auth headers"));
            }

            var claims = new[] {
                new Claim("CIN", cinClaim),
                new Claim("Role" , roleClaim),
                new Claim("subdomain" , tenantId)
            };
            
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            _logger.LogInformation("Authenticating user with CIN={CIN} for tenant {TenantId}", cinClaim, tenantId);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
