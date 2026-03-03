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

		// Configure HttpClient with API base URL
		var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7056";
		builder.Services.AddScoped(_ => new HttpClient
		{
			BaseAddress = new Uri(apiBaseUrl)
		});

		// Register API clients
		builder.Services.AddScoped<TaskApiClient>();
		builder.Services.AddScoped<PomodoroApiClient>();
		builder.Services.AddScoped<JournalApiClient>();
		builder.Services.AddScoped<SettingsApiClient>();
		builder.Services.AddScoped<TicketApiClient>();

		return builder.Build();
	}
}
