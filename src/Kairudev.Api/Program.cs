using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Kairudev.Application.Common;
using Kairudev.Application.Identity.Commands.GetOrCreateUser;
using Kairudev.Application.Journal.Commands.AddComment;
using Kairudev.Application.Journal.Commands.CreateEntry;
using Kairudev.Application.Journal.Commands.RemoveComment;
using Kairudev.Application.Journal.Commands.UpdateComment;
using Kairudev.Application.Journal.Queries.GetJournalByDate;
using Kairudev.Application.Journal.Queries.GetTodayJournal;
using Kairudev.Application.Pomodoro.Commands.CompleteSession;
using Kairudev.Application.Pomodoro.Commands.CreateTaskDuringSession;
using Kairudev.Application.Pomodoro.Commands.InterruptSession;
using Kairudev.Application.Pomodoro.Commands.LinkTask;
using Kairudev.Application.Pomodoro.Commands.SaveSettings;
using Kairudev.Application.Pomodoro.Commands.StartSession;
using Kairudev.Application.Pomodoro.Commands.UpdateTaskStatus;
using Kairudev.Application.Pomodoro.Queries.GetCurrentSession;
using Kairudev.Application.Pomodoro.Queries.GetSettings;
using Kairudev.Application.Pomodoro.Queries.GetSuggestedSessionType;
using Kairudev.Application.Settings.Commands.SaveJiraSettings;
using Kairudev.Application.Settings.Commands.SaveRingtonePreference;
using Kairudev.Application.Settings.Commands.SaveThemePreference;
using Kairudev.Application.Settings.Queries.GetUserSettings;
using Kairudev.Application.Tasks.Commands.AddTask;
using Kairudev.Application.Tasks.Commands.ChangeTaskStatus;
using Kairudev.Application.Tasks.Commands.CompleteTask;
using Kairudev.Application.Tasks.Commands.DeleteTask;
using Kairudev.Application.Tasks.Commands.LinkJiraTicket;
using Kairudev.Application.Tasks.Commands.UnlinkJiraTicket;
using Kairudev.Application.Tasks.Commands.UpdateTask;
using Kairudev.Application.Tasks.Queries.ListTasks;
using Kairudev.Application.Tickets.Queries.GetAssignedJiraTickets;
using Kairudev.Api.Auth;
using Kairudev.Infrastructure;
using Kairudev.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? "Data Source=kairudev.db";

builder.Services.AddInfrastructure(connectionString);

// Tasks — Command & Query Handlers
builder.Services.AddScoped<AddTaskCommandHandler>();
builder.Services.AddScoped<ListTasksQueryHandler>();
builder.Services.AddScoped<CompleteTaskCommandHandler>();
builder.Services.AddScoped<DeleteTaskCommandHandler>();
builder.Services.AddScoped<ChangeTaskStatusCommandHandler>();
builder.Services.AddScoped<UpdateTaskCommandHandler>();

// Pomodoro — Command & Query Handlers
builder.Services.AddScoped<GetSettingsQueryHandler>();
builder.Services.AddScoped<SaveSettingsCommandHandler>();
builder.Services.AddScoped<GetSuggestedSessionTypeQueryHandler>();
builder.Services.AddScoped<GetCurrentSessionQueryHandler>();
builder.Services.AddScoped<StartSessionCommandHandler>();
builder.Services.AddScoped<CompleteSessionCommandHandler>();
builder.Services.AddScoped<InterruptSessionCommandHandler>();
builder.Services.AddScoped<LinkTaskCommandHandler>();
builder.Services.AddScoped<CreateTaskDuringSessionCommandHandler>();
builder.Services.AddScoped<UpdateTaskStatusCommandHandler>();

// Journal — Command & Query Handlers
builder.Services.AddScoped<GetTodayJournalQueryHandler>();
builder.Services.AddScoped<GetJournalByDateQueryHandler>();
builder.Services.AddScoped<AddCommentCommandHandler>();
builder.Services.AddScoped<UpdateCommentCommandHandler>();
builder.Services.AddScoped<RemoveCommentCommandHandler>();
builder.Services.AddScoped<CreateEntryCommandHandler>(); // Used internally by Tasks & Pomodoro

// Settings — Command & Query Handlers
builder.Services.AddScoped<GetUserSettingsQueryHandler>();
builder.Services.AddScoped<SaveThemePreferenceCommandHandler>();
builder.Services.AddScoped<SaveRingtonePreferenceCommandHandler>();
builder.Services.AddScoped<SaveJiraSettingsCommandHandler>();

// Tickets — Query Handlers
builder.Services.AddScoped<GetAssignedJiraTicketsQueryHandler>();

// Tasks — Jira link/unlink handlers
builder.Services.AddScoped<LinkJiraTicketCommandHandler>();
builder.Services.AddScoped<UnlinkJiraTicketCommandHandler>();

// Identity
builder.Services.AddScoped<GetOrCreateUserCommandHandler>();

// HTTP Context
builder.Services.AddHttpContextAccessor();

// Current user service
builder.Services.AddScoped<ICurrentUserService, ClaimsCurrentUserService>();

// Authentication — JWT Bearer + GitHub OAuth
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"]
    ?? throw new InvalidOperationException("Jwt:SecretKey must be configured in appsettings or user secrets.");

var gitHubClientId = builder.Configuration["GitHub:ClientId"];
if (string.IsNullOrEmpty(gitHubClientId))
    throw new InvalidOperationException("GitHub:ClientId must be configured in appsettings or user secrets.");

var gitHubClientSecret = builder.Configuration["GitHub:ClientSecret"];
if (string.IsNullOrEmpty(gitHubClientSecret))
    throw new InvalidOperationException("GitHub:ClientSecret must be configured in appsettings or user secrets.");

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
                request.Headers.UserAgent.ParseAdd("Kairudev/1.0");
                using var response = await context.Backchannel.SendAsync(request, context.HttpContext.RequestAborted);
                response.EnsureSuccessStatusCode();
                var userJson = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                context.RunClaimActions(userJson.RootElement);
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("https://localhost:7204", "http://localhost:5010")
              .AllowAnyMethod()
              .AllowAnyHeader()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<KairudevDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapDefaultEndpoints();

app.Run();
