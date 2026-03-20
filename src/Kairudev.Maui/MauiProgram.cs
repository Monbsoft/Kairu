using Kairudev.Maui.Auth;
using Kairudev.Maui.Services;
using Microsoft.Extensions.Logging;

namespace Kairudev.Maui;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7056";

		// Auth services
		builder.Services.AddScoped<MauiAuthService>();
		builder.Services.AddScoped<AuthorizationMessageHandler>();

		// HttpClient avec handler d'autorisation Bearer
		builder.Services.AddHttpClient("KairudevApi", client =>
		{
			client.BaseAddress = new Uri(apiBaseUrl);
		})
		.AddHttpMessageHandler<AuthorizationMessageHandler>();

		// Fournir HttpClient depuis la factory pour les ApiClients qui l'injectent directement
		builder.Services.AddScoped(sp =>
			sp.GetRequiredService<IHttpClientFactory>().CreateClient("KairudevApi"));

		// Register API clients
		builder.Services.AddScoped<TaskApiClient>();
		builder.Services.AddScoped<PomodoroApiClient>();
		builder.Services.AddScoped<JournalApiClient>();
		builder.Services.AddScoped<SettingsApiClient>();
		builder.Services.AddScoped<TicketApiClient>();
		builder.Services.AddScoped<SprintApiClient>();
		builder.Services.AddSingleton<MarkdownService>();

		return builder.Build();
	}
}
