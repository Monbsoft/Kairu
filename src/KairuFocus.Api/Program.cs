using Azure.Core;
using Azure.Identity;
using KairuFocus.Api.Auth;
using KairuFocus.Api.Auth.Mcp;
using KairuFocus.Api.Generated;
using KairuFocus.Api.Mcp;
using KairuFocus.Application.Common;
using KairuFocus.Domain.Common;
using KairuFocus.Infrastructure;
using KairuFocus.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ModelContextProtocol.AspNetCore;
using Monbsoft.BrilliantMediator.Extensions;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Testing environment: disables SQL migration, prod-only validations, and prod Data Protection guard.
// Real configuration (JWT key, GitHub secrets, etc.) is injected by KairuFocusApiFactory.
var isTesting = builder.Environment.IsEnvironment("Testing");

// Get connection string: prioritize SQL_CONNECTION_STRING (Azure/production),
// then appsettings ConnectionStrings:Default (development).
// En env Testing, une chaîne fictive est acceptée : le DbContext sera remplacé par InMemory dans WebApplicationFactory.
var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? Environment.GetEnvironmentVariable("SQL_CONNECTION_STRING")
    ?? (isTesting
        ? "Server=(localdb)\\mssqllocaldb;Database=KairuFocus_Testing;Trusted_Connection=True;"
        : throw new InvalidOperationException(
            "A SQL Server connection string must be configured via 'ConnectionStrings:Default' or the 'SQL_CONNECTION_STRING' environment variable."));

builder.Services.AddInfrastructure(connectionString);

// Data Protection — clés partagées et persistées.
// En prod : Blob Storage (key ring) + Key Vault (chiffrement au repos).
// En dev : filesystem local implicite (~/.aspnet/DataProtection-Keys), acceptable localement.
var dpBuilder = builder.Services.AddDataProtection()
    .SetApplicationName("KairuFocus");

var dpBlobUri = builder.Configuration["DataProtection:BlobUri"];
var dpKeyVaultKeyUri = builder.Configuration["DataProtection:KeyVaultKeyUri"];

if (!string.IsNullOrEmpty(dpBlobUri) && !string.IsNullOrEmpty(dpKeyVaultKeyUri))
{
    // En prod : utiliser explicitement la UAMI via AZURE_CLIENT_ID injecté par le Container App.
    // ManagedIdentityCredential explicite évite la cascade de tentatives de DefaultAzureCredential
    // (env, VS, Azure CLI, etc.) — démarrage plus rapide et erreurs plus claires.
    // Fallback DefaultAzureCredential pour les scénarios dev avec Azure CLI/VS.
    var azureClientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID")
        ?? builder.Configuration["Azure:ManagedIdentityClientId"];
    TokenCredential credential = !string.IsNullOrEmpty(azureClientId)
        ? new ManagedIdentityCredential(azureClientId)
        : new DefaultAzureCredential();
    dpBuilder
        .PersistKeysToAzureBlobStorage(new Uri(dpBlobUri), credential)
        .ProtectKeysWithAzureKeyVault(new Uri(dpKeyVaultKeyUri), credential);
}
else if (!builder.Environment.IsDevelopment() && !isTesting)
{
    throw new InvalidOperationException(
        "DataProtection:BlobUri and DataProtection:KeyVaultKeyUri must be configured in production (non-Development environment).");
}
// Sinon (dev ou Testing sans config Azure) : PersistKeysToFileSystem implicite, acceptable localement.

// BrilliantMediator — handlers auto-découverts par le source generator
builder.Services.AddBrilliantMediator()
    .AddGeneratedHandlers()
    .Build();

// HTTP Context
builder.Services.AddHttpContextAccessor();

// Current user service
builder.Services.AddScoped<ICurrentUserService, ClaimsCurrentUserService>();

// JWT token generation (shared between Web auth and MCP OAuth)
builder.Services.AddSingleton<KairuFocus.Api.Auth.JwtTokenService>();

// Authentication — JWT Bearer + GitHub OAuth
// En env Testing, une clé fictive est utilisée (les tests n'émettent pas de vrais JWT).
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"]
    ?? (isTesting
        ? "testing-secret-key-minimum-32-chars-for-hmac"
        : throw new InvalidOperationException("Jwt:SecretKey must be configured in appsettings or user secrets."));

var gitHubClientId = builder.Configuration["GitHub:ClientId"] ?? string.Empty;
var gitHubClientSecret = builder.Configuration["GitHub:ClientSecret"] ?? string.Empty;

if (!builder.Environment.IsDevelopment() && !isTesting)
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
                request.Headers.UserAgent.ParseAdd("KairuFocus/1.0");
                using var response = await context.Backchannel.SendAsync(request, context.HttpContext.RequestAborted);
                response.EnsureSuccessStatusCode();
                var userJson = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                context.RunClaimActions(userJson.RootElement);
            }
        };
    })
    .AddScheme<AuthenticationSchemeOptions, McpTokenAuthenticationHandler>("McpToken", _ => { });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("McpBearer", policy =>
        policy.AddAuthenticationSchemes("McpToken")
              .RequireAuthenticatedUser());
});

builder.Services.AddControllers();
builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithTools<KairuFocusMcpTools>();
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

// Apply database migrations — skipped in Testing environment (in-memory DB used by integration tests).
if (!isTesting)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<KairuFocusDbContext>();
    Console.WriteLine("Applying database migrations...");
    await db.Database.MigrateAsync();
    Console.WriteLine("Database migrations applied successfully.");
}

if (!app.Environment.IsDevelopment())
{
    var forwardedOptions = new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    };
    // Azure App Service / Container Apps use non-loopback proxy IPs.
    // Clearing known networks/proxies lets ASP.NET Core trust X-Forwarded-Proto
    // from any upstream proxy, so the OAuth redirect_uri is built with https://.
    forwardedOptions.KnownNetworks.Clear();
    forwardedOptions.KnownProxies.Clear();
    app.UseForwardedHeaders(forwardedOptions);
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
app.MapMcp("/mcp").RequireAuthorization("McpBearer");
app.MapDefaultEndpoints();
app.MapFallbackToFile("index.html");

app.Services.UseBrilliantMediator();

app.Run();
