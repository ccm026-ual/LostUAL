using LostUAL.Web;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using LostUAL.Web.Auth;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;


var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped<BrowserStorage>();
builder.Services.AddScoped<TokenStore>();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<JwtAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<JwtAuthStateProvider>());

builder.Services.AddScoped<AuthHeaderHandler>();

/*builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("https://localhost:7178/")
});

await builder.Build().RunAsync();
*/

var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7178/";

builder.Services.AddHttpClient("Api", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
})
.AddHttpMessageHandler<AuthHeaderHandler>()
.AddHttpMessageHandler<UnauthorizedRedirectHandler>();

builder.Services.AddScoped<UnauthorizedRedirectHandler>();

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("Api"));

builder.Services.AddScoped<AuthService>();

await builder.Build().RunAsync();