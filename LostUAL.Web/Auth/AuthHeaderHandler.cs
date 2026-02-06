using System.Net.Http.Headers;

namespace LostUAL.Web.Auth;

public sealed class AuthHeaderHandler : DelegatingHandler
{
    private readonly TokenStore _tokenStore;
    public AuthHeaderHandler(TokenStore tokenStore) => _tokenStore = tokenStore;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        if (request.Headers.Authorization is null)
        {
            var token = await _tokenStore.GetAsync();
            if (!string.IsNullOrWhiteSpace(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, ct);
    }
}
