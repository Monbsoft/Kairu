using Kairudev.Web.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<Kairudev.Web.App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7056";

builder.Services.AddScoped(_ => new HttpClient
{
    BaseAddress = new Uri(apiBaseUrl)
});

builder.Services.AddScoped<TaskApiClient>();
builder.Services.AddScoped<PomodoroApiClient>();
builder.Services.AddScoped<JournalApiClient>();
builder.Services.AddScoped<SettingsApiClient>();
builder.Services.AddScoped<TicketApiClient>();
builder.Services.AddScoped<ISoundService, SoundService>();

await builder.Build().RunAsync();
