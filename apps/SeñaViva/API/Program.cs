using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using SenaViva.API.Auth;
using SenaViva.API.Data;
using SenaViva.API.Errors;
using SenaViva.API.Services;
using SenaViva.API.Swagger;

var builder = WebApplication.CreateBuilder(args);

var auth0 = Auth0Options.Load(builder.Configuration);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false)));

builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<UsersService>();
builder.Services.AddScoped<ConversationSessionsService>();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddAuth0Authentication(auth0);
builder.Services.AddSwaggerWithAuth0(auth0);

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerWithAuth0(auth0);
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
