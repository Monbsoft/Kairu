using KairuFocus.Web.Auth;
using KairuFocus.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using System.Globalization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<KairuFocus.Web.App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7056";

// Auth services
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<JwtAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<JwtAuthenticationStateProvider>());

// HttpClient avec handler d'autorisation Bearer
builder.Services.AddScoped<AuthorizationMessageHandler>();
builder.Services.AddHttpClient("KairuFocusApi", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
})
.AddHttpMessageHandler<AuthorizationMessageHandler>();

// Fournir HttpClient depuis la factory pour les ApiClients qui l'injectent directement
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("KairuFocusApi"));

builder.Services.AddScoped<TaskApiClient>();
builder.Services.AddScoped<PomodoroApiClient>();
builder.Services.AddScoped<JournalApiClient>();
builder.Services.AddScoped<SettingsApiClient>();
builder.Services.AddScoped<McpTokenService>();
builder.Services.AddScoped<ISoundService, SoundService>();
builder.Services.AddSingleton<MarkdownService>();
builder.Services.AddLocalization();

var host = builder.Build();

const string defaultCulture = "fr";

var js = host.Services.GetRequiredService<IJSRuntime>();
var result = await js.InvokeAsync<string>("blazorCulture.get");
var culture = CultureInfo.GetCultureInfo(result ?? defaultCulture);

if (result == null)
{
    await js.InvokeVoidAsync("blazorCulture.set", defaultCulture);
}

CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

await host.RunAsync();
