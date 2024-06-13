using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Encodings.Web;

namespace FilePicker.Web.Handlers
{
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly ConnectionStrings _connectionStrings;

        public BasicAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        ConnectionStrings connectionStrings)
        : base(options, logger, encoder, clock)
        {
            _connectionStrings = connectionStrings;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return AuthenticateResult.Fail("Authorization header not found");
            }

            try
            {
                var authenticationHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                var credentialBytes = Convert.FromBase64String(authenticationHeader.Parameter);
                var credentials = System.Text.Encoding.UTF8.GetString(credentialBytes).Split(':');
                var username = credentials[0];
                var password = credentials[1];

                // Aqui você pode verificar o username e a senha em algum serviço ou banco de dados
                // Por simplicidade, este exemplo compara com valores hardcoded
                if (username == _connectionStrings.UserApi && password == _connectionStrings.PasswordApi)
                {
                    var identity = new System.Security.Claims.ClaimsIdentity(Scheme.Name);
                    var principal = new System.Security.Claims.ClaimsPrincipal(identity);
                    var ticket = new AuthenticationTicket(principal, Scheme.Name);
                    return AuthenticateResult.Success(ticket);
                }
                else
                {
                    return AuthenticateResult.Fail("Invalid username or password");
                }
            }
            catch
            {
                return AuthenticateResult.Fail("Invalid authorization header");
            }
        }
    }
}
