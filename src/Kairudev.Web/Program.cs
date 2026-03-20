using Kairudev.Web.Auth;
using Kairudev.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<Kairudev.Web.App>("#app");
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
builder.Services.AddHttpClient("KairudevApi", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
})
.AddHttpMessageHandler<AuthorizationMessageHandler>();

// Fournir HttpClient depuis la factory pour les ApiClients qui l'injectent directement
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("KairudevApi"));

builder.Services.AddScoped<TaskApiClient>();
builder.Services.AddScoped<PomodoroApiClient>();
builder.Services.AddScoped<JournalApiClient>();
builder.Services.AddScoped<SettingsApiClient>();
builder.Services.AddScoped<TicketApiClient>();
builder.Services.AddScoped<SprintApiClient>();
builder.Services.AddScoped<ISoundService, SoundService>();
builder.Services.AddSingleton<MarkdownService>();

await builder.Build().RunAsync();
