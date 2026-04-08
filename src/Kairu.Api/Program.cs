using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Kairu.Application.Common;
using Kairu.Api.Auth;
using Kairu.Api.Generated;
using Kairu.Api.Mcp;
using Kairu.Domain.Common;
using Kairu.Infrastructure;
using Kairu.Infrastructure.Persistence;
using Monbsoft.BrilliantMediator.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ModelContextProtocol.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Get connection string: prioritize SQL_CONNECTION_STRING (Azure/production),
// then appsettings ConnectionStrings:Default (development)
var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? Environment.GetEnvironmentVariable("SQL_CONNECTION_STRING")
    ?? throw new InvalidOperationException(
        "A SQL Server connection string must be configured via 'ConnectionStrings:Default' or the 'SQL_CONNECTION_STRING' environment variable.");

builder.Services.AddInfrastructure(connectionString);

// BrilliantMediator — handlers auto-découverts par le source generator
builder.Services.AddBrilliantMediator()
    .AddGeneratedHandlers()
    .Build();

// HTTP Context
builder.Services.AddHttpContextAccessor();

// Current user service
builder.Services.AddScoped<ICurrentUserService, ClaimsCurrentUserService>();

// JWT token generation (shared between Web auth and MCP OAuth)
builder.Services.AddSingleton<Kairu.Api.Auth.JwtTokenService>();

// Authentication — JWT Bearer + GitHub OAuth
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"]
    ?? throw new InvalidOperationException("Jwt:SecretKey must be configured in appsettings or user secrets.");

var gitHubClientId = builder.Configuration["GitHub:ClientId"] ?? string.Empty;
var gitHubClientSecret = builder.Configuration["GitHub:ClientSecret"] ?? string.Empty;

if (!builder.Environment.IsDevelopment())
{
    if (string.IsNullOrEmpty(gitHubClientId))
        throw new InvalidOperationException("GitHub:ClientId must be configured in appsettings or user secrets.");
    if (string.IsNullOrEmpty(gitHubClientSecret))
        throw new InvalidOperationException("GitHub:ClientSecret must be configured in appsettings or user secrets.");
}
else if (string.IsNullOrEmpty(gitHubClientId) || string.IsNullOrEmpty(gitHubClientSecret))
{
    Console.Error.WriteLine("WARNING: GitHub:ClientId or GitHub:ClientSecret is not configured. GitHub OAuth login will not function.");
}

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie()
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
    })
    .AddOAuth("GitHub", options =>
    {
        options.ClientId = gitHubClientId;
        options.ClientSecret = gitHubClientSecret;
        options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
        options.TokenEndpoint = "https://github.com/login/oauth/access_token";
        options.UserInformationEndpoint = "https://api.github.com/user";
        options.CallbackPath = "/signin-github";
        options.Scope.Add("user:email");
        options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
        options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
        options.ClaimActions.MapJsonKey("urn:github:login", "login");
        options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
        options.Events = new Microsoft.AspNetCore.Authentication.OAuth.OAuthEvents
        {
            OnCreatingTicket = async context =>
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", context.AccessToken);
                request.Headers.UserAgent.ParseAdd("Kairu/1.0");
                using var response = await context.Backchannel.SendAsync(request, context.HttpContext.RequestAborted);
                response.EnsureSuccessStatusCode();
                var userJson = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                context.RunClaimActions(userJson.RootElement);
            }
        };
    })
    .AddMcp(options =>
    {
        // Resource metadata returned in 401 challenges on /mcp
        // AuthorizationServers will be set dynamically per-request
        options.Events = new ModelContextProtocol.AspNetCore.Authentication.McpAuthenticationEvents
        {
            OnResourceMetadataRequest = context =>
            {
                var baseUrl = $"{context.Request.Scheme}://{context.Request.Host}";
                context.ResourceMetadata = new ModelContextProtocol.Authentication.ProtectedResourceMetadata
                {
                    Resource = $"{baseUrl}/mcp",
                    AuthorizationServers = [baseUrl],
                    BearerMethodsSupported = ["header"],
                };
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("McpOAuth", policy =>
        policy.AddAuthenticationSchemes(
            JwtBearerDefaults.AuthenticationScheme,
            ModelContextProtocol.AspNetCore.Authentication.McpAuthenticationDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser());
});

builder.Services.AddControllers();
builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithTools<KairuMcpTools>();
builder.Services.AddOpenApi();

var allowedOrigins = builder.Configuration
    .GetSection("AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()));

var app = builder.Build();

// Apply database migrations safely
try
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<KairuDbContext>();
        await db.Database.MigrateAsync();
    }
}
catch (Exception ex)
{
    Console.Error.WriteLine($"ERROR: Failed to apply migrations: {ex.Message}");
    // Log the error but don't crash the app if migrations fail
    // This can happen if the database connection is not ready yet
    if (!builder.Environment.IsProduction())
        throw;
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();
if (app.Environment.IsDevelopment())
    app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions
{
    ServeUnknownFileTypes = true,
    DefaultContentType = "application/octet-stream"
});
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapMcp("/mcp").RequireAuthorization("McpOAuth");
app.MapDefaultEndpoints();
app.MapFallbackToFile("index.html");

app.Services.UseBrilliantMediator();

app.Run();
