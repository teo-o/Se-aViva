using System.Security.Claims;
using SenaViva.Shared.Auth;

namespace SenaViva.API.Auth;

public record CurrentUser(string Auth0Id, string? Email, string? FullName, IReadOnlyList<string> Roles)
{
    // Client-credentials (machine-to-machine) tokens use "<client_id>@clients" as subject.
    public bool IsMachine => Auth0Id.EndsWith("@clients", StringComparison.Ordinal);

    public bool IsAdministrator => Roles.Contains(SenaVivaClaims.AdministratorRole);

    public static CurrentUser From(ClaimsPrincipal principal) =>
        new(
            principal.FindFirstValue("sub") ?? throw new InvalidOperationException("The access token has no 'sub' claim."),
            principal.FindFirstValue(SenaVivaClaims.Email),
            principal.FindFirstValue(SenaVivaClaims.FullName),
            principal.FindAll(SenaVivaClaims.Roles).Select(claim => claim.Value).ToList());
}
