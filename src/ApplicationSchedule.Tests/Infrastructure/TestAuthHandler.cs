using System.Security.Claims;
using System.Text.Encodings.Web;
using ApplicationSchedule.Application.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ApplicationSchedule.Tests.Infrastructure;

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Test";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (Request.Headers.TryGetValue("X-Test-Anonymous", out var anonymousHeader) &&
            anonymousHeader.ToString().Equals("true", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        string rol = RolesSistema.Administrador;

        if (Request.Headers.TryGetValue("X-Test-Role", out var roleHeader) &&
            !string.IsNullOrWhiteSpace(roleHeader.ToString()))
        {
            rol = roleHeader.ToString();
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "test-user"),
            new Claim(ClaimTypes.Name, "Usuario Pruebas"),
            new Claim(ClaimTypes.Email, "tests@applicationschedule.local"),
            new Claim(ClaimTypes.Role, rol)
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}