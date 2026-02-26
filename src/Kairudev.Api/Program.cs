using Kairudev.Application.Journal.CreateJournalEntry;
using Kairudev.Domain.Journal;
using Kairudev.Infrastructure;
using Kairudev.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? "Data Source=kairudev.db";

builder.Services.AddInfrastructure(connectionString);

// Journal — ICreateJournalEntryUseCase avec NoOpPresenter (usage interne par les autres interactors)
builder.Services.AddScoped<ICreateJournalEntryUseCase>(sp =>
    new CreateJournalEntryInteractor(
        sp.GetRequiredService<IJournalEntryRepository>(),
        new NoOpJournalEntryPresenter()));

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
