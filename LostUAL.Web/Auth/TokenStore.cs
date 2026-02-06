namespace LostUAL.Web.Auth;

public sealed class TokenStore
{
    private const string TokenKey = "auth.jwt";
    private readonly BrowserStorage _storage;

    public TokenStore(BrowserStorage storage) => _storage = storage;

    public async Task<string?> GetAsync() => await _storage.GetAsync(TokenKey);
    public Task SetAsync(string token) => _storage.SetAsync(TokenKey, token).AsTask();
    public Task ClearAsync() => _storage.RemoveAsync(TokenKey).AsTask();
}
