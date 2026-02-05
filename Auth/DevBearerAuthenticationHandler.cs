using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Api.Comercial.Auth;

public sealed class DevBearerAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private const string SchemeName = "DevBearer";

    public DevBearerAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        string? token = null;

        if (Request.Headers.TryGetValue("Authorization", out var authHeaderValues))
        {
            var authHeader = authHeaderValues.ToString();
            if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization header scheme."));
            }

            token = authHeader["Bearer ".Length..].Trim();
            if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = token["Bearer ".Length..].Trim();
            }
        }
        else if (Request.Query.TryGetValue("dev_token", out var devTokenValues))
        {
            token = devTokenValues.ToString();
        }
        else
        {
            return Task.FromResult(AuthenticateResult.Fail("Missing Authorization header."));
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            return Task.FromResult(AuthenticateResult.Fail("Token is empty."));
        }
        var expectedToken = Context.RequestServices.GetRequiredService<IConfiguration>()["DevAuth:Token"];
        if (string.IsNullOrWhiteSpace(expectedToken))
        {
            return Task.FromResult(AuthenticateResult.Fail("DevAuth:Token is not configured."));
        }

        if (!string.Equals(token, expectedToken, StringComparison.Ordinal))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid token."));
        }

        var config = Context.RequestServices.GetRequiredService<IConfiguration>();
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, config["DevAuth:Name"] ?? "Dev User"),
            new("preferred_username", config["DevAuth:PreferredUsername"] ?? "dev.user@local"),
            new("oid", config["DevAuth:Oid"] ?? Guid.NewGuid().ToString()),
            new("sub", config["DevAuth:Sub"] ?? Guid.NewGuid().ToString()),
            new("tid", config["DevAuth:TenantId"] ?? "dev-tenant")
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
