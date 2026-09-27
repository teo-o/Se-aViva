using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;

namespace SenaViva.Web.Auth;

// Reading the token from HttpContext is safe because the app only uses static server-side rendering:
// every API call happens while handling an HTTP request, never inside an interactive circuit.
public class AccessTokenHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (request.Headers.Authorization is null)
        {
            var httpContext = httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("The SeñaViva API can only be called while handling an HTTP request.");

            var accessToken = await httpContext.GetTokenAsync("access_token");
            if (!string.IsNullOrEmpty(accessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
