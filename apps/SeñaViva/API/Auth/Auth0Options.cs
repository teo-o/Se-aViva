namespace SenaViva.API.Auth;

public class Auth0Options
{
    public const string SectionName = "Auth0";

    public string Domain { get; init; } = string.Empty;

    public string Audience { get; init; } = string.Empty;

    public string SwaggerClientId { get; init; } = string.Empty;

    public string Authority => $"https://{Domain}/";

    public static Auth0Options Load(IConfiguration configuration)
    {
        var options = configuration.GetSection(SectionName).Get<Auth0Options>() ?? new Auth0Options();

        if (string.IsNullOrWhiteSpace(options.Domain) || string.IsNullOrWhiteSpace(options.Audience))
        {
            throw new InvalidOperationException(
                "Auth0 is not configured. Set 'Auth0:Domain' and 'Auth0:Audience' (see README.md).");
        }

        return options;
    }
}
