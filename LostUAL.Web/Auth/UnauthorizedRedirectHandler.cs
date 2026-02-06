using System.Net;
using System.Threading;
using Microsoft.AspNetCore.Components;

namespace LostUAL.Web.Auth;

public sealed class UnauthorizedRedirectHandler : DelegatingHandler
{
    private readonly JwtAuthStateProvider _authState;
    private readonly NavigationManager _nav;

    private int _redirecting = 0;

    public UnauthorizedRedirectHandler(JwtAuthStateProvider authState, NavigationManager nav)
    {
        _authState = authState;
        _nav = nav;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var response = await base.SendAsync(request, ct);

        if (response.StatusCode != HttpStatusCode.Unauthorized)
            return response;

        var path = request.RequestUri?.AbsolutePath?.ToLowerInvariant();
        if (path is not null &&
            (path.EndsWith("/api/Auth/login") || path.EndsWith("/api/Auth/register")))
        {
            return response;
        }

        if (Interlocked.Exchange(ref _redirecting, 1) == 0)
        {
            await _authState.ClearTokenAsync();

            var returnUrl = Uri.EscapeDataString(_nav.Uri);
            _nav.NavigateTo($"/login?returnUrl={returnUrl}", replace: true);
        }

        return response;
    }
}
