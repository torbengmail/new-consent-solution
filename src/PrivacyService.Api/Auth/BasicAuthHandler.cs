using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using PrivacyConsent.Domain.Models;

namespace PrivacyService.Api.Auth;

public class BasicAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IAccessControlService _accessControl;

    public BasicAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IAccessControlService accessControl)
        : base(options, logger, encoder)
    {
        _accessControl = accessControl;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
            return AuthenticateResult.NoResult();

        try
        {
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers.Authorization!);
            if (authHeader.Scheme != "Basic")
                return AuthenticateResult.NoResult();

            var credentialBytes = Convert.FromBase64String(authHeader.Parameter ?? "");
            var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':', 2);
            var username = credentials[0];
            var password = credentials[1];

            var identity = await _accessControl.AuthenticateBasicAsync(username, password);
            if (identity == null)
                return AuthenticateResult.Fail("Invalid username or password");

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, identity.Username),
                new("user_id", identity.Id.ToString()),
            };

            foreach (var permission in identity.Permissions)
            {
                var normalized = permission.ToLowerInvariant().Replace('_', '-');
                claims.Add(new Claim("permission", normalized));
            }
            foreach (var owner in identity.Owners)
                claims.Add(new Claim("owner", owner.ToString()));

            var claimsIdentity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(claimsIdentity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
        catch
        {
            return AuthenticateResult.Fail("Invalid authorization header");
        }
    }
}
