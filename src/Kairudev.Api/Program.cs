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
using Kairudev.Application.Settings.Commands.SaveRingtonePreference;
using Kairudev.Application.Settings.Commands.SaveThemePreference;
using Kairudev.Application.Settings.Queries.GetUserSettings;
using Kairudev.Application.Tasks.Commands.AddTask;
using Kairudev.Application.Tasks.Commands.ChangeTaskStatus;
using Kairudev.Application.Tasks.Commands.CompleteTask;
using Kairudev.Application.Tasks.Commands.DeleteTask;
using Kairudev.Application.Tasks.Commands.UpdateTask;
using Kairudev.Application.Tasks.Queries.ListTasks;
using Kairudev.Infrastructure;
using Kairudev.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

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
app.MapControllers();
app.MapDefaultEndpoints();

app.Run();
