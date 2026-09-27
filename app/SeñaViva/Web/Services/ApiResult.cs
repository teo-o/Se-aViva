using System.Net;

namespace SenaViva.Web.Services;

// StatusCode is null when the API could not be reached at all.
public record ApiError(HttpStatusCode? StatusCode, string? Detail, IReadOnlyList<string> ValidationErrors);

public record ApiResult(ApiError? Error)
{
    public bool Succeeded => Error is null;
}

public record ApiResult<T>(T? Value, ApiError? Error) : ApiResult(Error);
