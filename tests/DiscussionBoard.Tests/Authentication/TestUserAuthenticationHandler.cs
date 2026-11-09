using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DiscussionBoard.Tests.Authentication;

/// <summary>
/// Authenticates integration test requests using test headers.
/// </summary>
public sealed class TestUserAuthenticationHandler
    : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public new const string Scheme = "IntegrationTestUser";
    public const string UserHeader = "X-Integration-Test-User-Id";
    public const string RoleHeader = "X-Integration-Test-Role";

    public TestUserAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(UserHeader, out var userIds) ||
            string.IsNullOrWhiteSpace(userIds.ToString()))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var id = userIds.ToString();

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, id),
            new Claim(ClaimTypes.Name, id)
        };

        if (Request.Headers.TryGetValue(RoleHeader, out var roles))
        {
            foreach (var role in roles.ToString().Split(
                         ',',
                         StringSplitOptions.RemoveEmptyEntries |
                         StringSplitOptions.TrimEntries))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }

        var identity = new ClaimsIdentity(claims, Scheme);

        var principal = new ClaimsPrincipal(identity);

        var ticket = new AuthenticationTicket(principal, Scheme);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
