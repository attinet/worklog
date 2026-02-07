using System.Net.Http.Headers;

namespace WorkLog.Web.Services;

/// <summary>
/// HTTP 請求攔截器，自動附加 JWT Token
/// </summary>
public class AuthTokenHandler : DelegatingHandler
{
    private readonly AuthService _authService;

    public AuthTokenHandler(AuthService authService)
    {
        _authService = authService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _authService.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await base.SendAsync(request, cancellationToken);

        // 如果收到 401，嘗試刷新 Token 後重試
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            var newToken = await _authService.TryRefreshToken();
            if (!string.IsNullOrEmpty(newToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
                response = await base.SendAsync(request, cancellationToken);
            }
        }

        return response;
    }
}
