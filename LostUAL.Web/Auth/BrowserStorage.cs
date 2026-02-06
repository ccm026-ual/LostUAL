using Microsoft.JSInterop;

namespace LostUAL.Web.Auth;

public sealed class BrowserStorage
{
    private readonly IJSRuntime _js;
    public BrowserStorage(IJSRuntime js) => _js = js;

    public ValueTask<string?> GetAsync(string key)
        => _js.InvokeAsync<string?>("appStorage.get", key);

    public ValueTask SetAsync(string key, string value)
        => _js.InvokeVoidAsync("appStorage.set", key, value);

    public ValueTask RemoveAsync(string key)
        => _js.InvokeVoidAsync("appStorage.remove", key);
}
