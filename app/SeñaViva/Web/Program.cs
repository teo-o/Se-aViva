using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using SenaViva.Web.Auth;
using SenaViva.Web.Components;
using SenaViva.Web.Services;

var builder = WebApplication.CreateBuilder(args);

var auth0 = WebAuth0Options.Load(builder.Configuration);
var apiBaseUrl = builder.Configuration["SenaVivaApi:BaseUrl"]
    ?? throw new InvalidOperationException("SenaVivaApi:BaseUrl is not configured.");

builder.Services
    .AddAuth0WebAppAuthentication(options =>
    {
        options.Domain = auth0.Domain;
        options.ClientId = auth0.ClientId;
        options.ClientSecret = auth0.ClientSecret;
        options.Scope = "openid profile email";
        options.OpenIdConnectEvents = new OpenIdConnectEvents
        {
            OnTicketReceived = NewUserRegistration.RegisterAsync
        };
    })
    .WithAccessToken(options => options.Audience = auth0.Audience);

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<AccessTokenHandler>();
builder.Services
    .AddHttpClient<SenaVivaApiClient>(client => client.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<AccessTokenHandler>();

builder.Services.AddRazorComponents();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapAccountEndpoints();
app.MapRazorComponents<App>();

app.Run();
