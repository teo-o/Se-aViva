using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using SenaViva.Shared.DTOs.Requests;
using SenaViva.Shared.DTOs.Responses;

namespace SenaViva.Web.Services;

public class SenaVivaApiClient(HttpClient httpClient, ILogger<SenaVivaApiClient> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public Task<ApiResult<UserResponse>> GetCurrentUserAsync() =>
        SendAsync<UserResponse>(HttpMethod.Get, "api/users/me");

    // accessToken is only passed right after login, before the authentication cookie exists.
    public Task<ApiResult<UserResponse>> RegisterCurrentUserAsync(RegisterCurrentUserRequest request, string? accessToken = null) =>
        SendAsync<UserResponse>(HttpMethod.Post, "api/users/me", request, accessToken);

    public Task<ApiResult<List<UserResponse>>> GetUsersAsync() =>
        SendAsync<List<UserResponse>>(HttpMethod.Get, "api/users");

    public Task<ApiResult<UserResponse>> GetUserAsync(int id) =>
        SendAsync<UserResponse>(HttpMethod.Get, $"api/users/{id}");

    public Task<ApiResult<UserResponse>> CreateUserAsync(CreateUserRequest request) =>
        SendAsync<UserResponse>(HttpMethod.Post, "api/users", request);

    public Task<ApiResult> UpdateUserAsync(int id, UpdateUserRequest request) =>
        SendAsync(HttpMethod.Put, $"api/users/{id}", request);

    public Task<ApiResult> DeleteUserAsync(int id) =>
        SendAsync(HttpMethod.Delete, $"api/users/{id}");

    public Task<ApiResult<List<ConversationSessionResponse>>> GetSessionsAsync() =>
        SendAsync<List<ConversationSessionResponse>>(HttpMethod.Get, "api/conversation-sessions");

    public Task<ApiResult<ConversationSessionResponse>> GetSessionAsync(Guid id) =>
        SendAsync<ConversationSessionResponse>(HttpMethod.Get, $"api/conversation-sessions/{id}");

    public Task<ApiResult<ConversationSessionResponse>> CreateSessionAsync(CreateConversationSessionRequest request) =>
        SendAsync<ConversationSessionResponse>(HttpMethod.Post, "api/conversation-sessions", request);

    public Task<ApiResult> UpdateSessionAsync(Guid id, UpdateConversationSessionRequest request) =>
        SendAsync(HttpMethod.Put, $"api/conversation-sessions/{id}", request);

    public Task<ApiResult> DeleteSessionAsync(Guid id) =>
        SendAsync(HttpMethod.Delete, $"api/conversation-sessions/{id}");

    private async Task<ApiResult<T>> SendAsync<T>(HttpMethod method, string uri, object? body = null, string? accessToken = null)
    {
        var (response, error) = await TrySendAsync(method, uri, body, accessToken);
        if (response is null)
        {
            return new ApiResult<T>(default, error);
        }

        using (response)
        {
            var value = await response.Content.ReadFromJsonAsync<T>(JsonOptions);
            return new ApiResult<T>(value, null);
        }
    }

    private async Task<ApiResult> SendAsync(HttpMethod method, string uri, object? body = null)
    {
        var (response, error) = await TrySendAsync(method, uri, body, null);
        response?.Dispose();
        return new ApiResult(error);
    }

    private async Task<(HttpResponseMessage? Response, ApiError? Error)> TrySendAsync(
        HttpMethod method,
        string uri,
        object? body,
        string? accessToken)
    {
        using var request = new HttpRequestMessage(method, uri);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body, options: JsonOptions);
        }

        if (accessToken is not null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        try
        {
            var response = await httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                return (response, null);
            }

            using (response)
            {
                return (null, await ReadErrorAsync(response));
            }
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "Could not reach the SeñaViva API at {BaseAddress}", httpClient.BaseAddress);
            return (null, new ApiError(null, exception.Message, []));
        }
    }

    private static async Task<ApiError> ReadErrorAsync(HttpResponseMessage response)
    {
        HttpValidationProblemDetails? problem = null;
        var mediaType = response.Content.Headers.ContentType?.MediaType;
        if (mediaType is not null && mediaType.EndsWith("json", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>(JsonOptions);
            }
            catch (JsonException)
            {
                // A non-ProblemDetails body still yields a usable error from the status code.
            }
        }

        var validationErrors = problem?.Errors.SelectMany(error => error.Value).ToList() ?? [];
        return new ApiError(response.StatusCode, problem?.Detail, validationErrors);
    }
}
