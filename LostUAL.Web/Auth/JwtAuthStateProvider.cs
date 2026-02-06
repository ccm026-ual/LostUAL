using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace LostUAL.Web.Auth;

public sealed class JwtAuthStateProvider : AuthenticationStateProvider
{
    private readonly TokenStore _tokenStore;
    public JwtAuthStateProvider(TokenStore tokenStore) => _tokenStore = tokenStore;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _tokenStore.GetAsync();
        if (string.IsNullOrWhiteSpace(token))
            return Anonymous();

        try
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            var identity = new ClaimsIdentity(jwt.Claims, "jwt");
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
        catch
        {
            await _tokenStore.ClearAsync();
            return Anonymous();
        }
    }

    public async Task SetTokenAsync(string token)
    {
        await _tokenStore.SetAsync(token);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task ClearTokenAsync()
    {
        await _tokenStore.ClearAsync();
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    private static AuthenticationState Anonymous()
        => new(new ClaimsPrincipal(new ClaimsIdentity()));
}
