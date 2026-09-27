namespace SenaViva.Web.Auth;

public class WebAuth0Options
{
    public const string SectionName = "Auth0";

    public string Domain { get; init; } = string.Empty;

    public string ClientId { get; init; } = string.Empty;

    public string ClientSecret { get; init; } = string.Empty;

    public string Audience { get; init; } = string.Empty;

    public static WebAuth0Options Load(IConfiguration configuration)
    {
        var options = configuration.GetSection(SectionName).Get<WebAuth0Options>() ?? new WebAuth0Options();

        var missing = new Dictionary<string, string>
            {
                ["Auth0:Domain"] = options.Domain,
                ["Auth0:ClientId"] = options.ClientId,
                ["Auth0:ClientSecret"] = options.ClientSecret,
                ["Auth0:Audience"] = options.Audience
            }
            .Where(setting => string.IsNullOrWhiteSpace(setting.Value))
            .Select(setting => setting.Key)
            .ToList();

        if (missing.Count > 0)
        {
            throw new InvalidOperationException(
                $"Auth0 is not configured for the web app. Missing: {string.Join(", ", missing)} (see README.md).");
        }

        return options;
    }
}
