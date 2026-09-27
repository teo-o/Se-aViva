using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace SenaViva.Web.Auth;

public static class AccountEndpoints
{
    public static IEndpointRouteBuilder MapAccountEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var account = endpoints.MapGroup("/Account");

        account.MapGet("/Login", async (HttpContext httpContext, string? returnUrl, bool signup = false) =>
        {
            var properties = new LoginAuthenticationPropertiesBuilder().WithRedirectUri(LocalUrlOrRoot(returnUrl));
            if (signup)
            {
                properties = properties.WithParameter("screen_hint", "signup");
            }

            await httpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, properties.Build());
        });

        account.MapPost("/Logout", async (HttpContext httpContext, IAntiforgery antiforgery) =>
        {
            if (!await antiforgery.IsRequestValidAsync(httpContext))
            {
                return Results.BadRequest();
            }

            var properties = new LogoutAuthenticationPropertiesBuilder().WithRedirectUri("/").Build();
            await httpContext.SignOutAsync(Auth0Constants.AuthenticationScheme, properties);
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.Empty;
        });

        return endpoints;
    }

    private static string LocalUrlOrRoot(string? url) =>
        url is { Length: > 0 } && url[0] == '/' && !url.StartsWith("//") && !url.StartsWith("/\\") ? url : "/";
}
