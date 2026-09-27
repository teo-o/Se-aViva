using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using SenaViva.Shared.Auth;
using SenaViva.Shared.DTOs.Requests;
using SenaViva.Shared.Enums;
using SenaViva.Web.Services;

namespace SenaViva.Web.Auth;

// New sign-ups answer "¿Cómo te comunicas?" in the Auth0 form, so their SeñaViva user is created right after login.
// Existing users and administrators never get that claim and are asked on the profile page instead.
public static class NewUserRegistration
{
    public static async Task RegisterAsync(TicketReceivedContext context)
    {
        var principal = context.Principal;
        var accessToken = context.Properties?.GetTokenValue("access_token");
        if (principal is null || accessToken is null || principal.HasClaim(SenaVivaClaims.Roles, SenaVivaClaims.AdministratorRole))
        {
            return;
        }

        var role = principal.FindFirstValue(SenaVivaClaims.UserType) switch
        {
            nameof(UserRole.Deaf) => UserRole.Deaf,
            nameof(UserRole.Hearing) => UserRole.Hearing,
            _ => (UserRole?)null
        };

        if (role is null)
        {
            return;
        }

        var api = context.HttpContext.RequestServices.GetRequiredService<SenaVivaApiClient>();
        var result = await api.RegisterCurrentUserAsync(new RegisterCurrentUserRequest { Role = role }, accessToken);

        if (!result.Succeeded)
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<SenaVivaApiClient>>();
            logger.LogWarning(
                "Could not register {Auth0Id} in SeñaViva after login ({StatusCode}: {Detail})",
                principal.FindFirstValue("sub"),
                result.Error?.StatusCode,
                result.Error?.Detail);
        }
    }
}
