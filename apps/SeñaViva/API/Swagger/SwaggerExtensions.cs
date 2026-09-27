using Microsoft.OpenApi;
using SenaViva.API.Auth;

namespace SenaViva.API.Swagger;

public static class SwaggerExtensions
{
    private const string OAuth2Scheme = "Auth0";
    private const string BearerScheme = "Bearer";

    public static IServiceCollection AddSwaggerWithAuth0(this IServiceCollection services, Auth0Options auth0)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "SeñaViva API",
                Version = "v1",
                Description = "Users and conversation sessions for SeñaViva (LSC ⇄ Spanish)."
            });

            options.AddSecurityDefinition(OAuth2Scheme, new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Description = "Sign in with an Auth0 test user (Authorization Code + PKCE).",
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri($"{auth0.Authority}authorize"),
                        TokenUrl = new Uri($"{auth0.Authority}oauth/token"),
                        Scopes = new Dictionary<string, string>
                        {
                            ["openid"] = "OpenID Connect",
                            ["profile"] = "User profile",
                            ["email"] = "User email"
                        }
                    }
                }
            });

            options.AddSecurityDefinition(BearerScheme, new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Paste an Auth0 access token (without the 'Bearer ' prefix)."
            });

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(OAuth2Scheme, document)] = ["openid", "profile", "email"]
            });
            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(BearerScheme, document)] = []
            });
        });

        return services;
    }

    public static WebApplication UseSwaggerWithAuth0(this WebApplication app, Auth0Options auth0)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.DocumentTitle = "SeñaViva API";
            options.OAuthClientId(auth0.SwaggerClientId);
            options.OAuthUsePkce();
            options.OAuthScopes("openid", "profile", "email");
            options.OAuthAdditionalQueryStringParams(new Dictionary<string, string> { ["audience"] = auth0.Audience });
        });

        return app;
    }
}
