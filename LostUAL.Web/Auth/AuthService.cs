using System.Net.Http.Json;

namespace LostUAL.Web.Auth;

public sealed class AuthService
{
    private readonly HttpClient _http;
    private readonly JwtAuthStateProvider _authState;

    public AuthService(HttpClient http, JwtAuthStateProvider authState)
    {
        _http = http;
        _authState = authState;
    }

    public async Task<(bool Ok, string? Error)> LoginAsync(string email, string password)
    {
        var resp = await _http.PostAsJsonAsync("/api/auth/login",
            new LoginRequest { Email = email, Password = password });

        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync();
            return (false, $"Login falló: {(int)resp.StatusCode} {resp.ReasonPhrase}. {body}");
        }

        var data = await resp.Content.ReadFromJsonAsync<LoginResponse>();
        if (data is null || string.IsNullOrWhiteSpace(data.Token))
            return (false, "Login OK pero no llegó 'token' en la respuesta.");

        await _authState.SetTokenAsync(data.Token);
        return (true, null);
    }

    public Task LogoutAsync() => _authState.ClearTokenAsync();
}
