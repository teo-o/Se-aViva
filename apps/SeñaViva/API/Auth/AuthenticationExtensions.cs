using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace SenaViva.API.Auth;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddAuth0Authentication(this IServiceCollection services, Auth0Options auth0)
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = auth0.Authority;
                options.Audience = auth0.Audience;
                options.MapInboundClaims = false;
                options.TokenValidationParameters.NameClaimType = "sub";
            });

        var authorization = services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

        foreach (var permission in Permissions.All)
        {
            authorization.AddPolicy(permission, policy =>
                policy.RequireAssertion(context => HasPermission(context.User, permission)));
        }

        return services;
    }

    // Auth0 emits RBAC permissions in the "permissions" claim for user tokens,
    // while client-credentials tokens may only carry them in the space-separated "scope" claim.
    private static bool HasPermission(ClaimsPrincipal user, string permission) =>
        user.HasClaim("permissions", permission) ||
        user.FindAll("scope").Any(claim => claim.Value.Split(' ').Contains(permission));
}
