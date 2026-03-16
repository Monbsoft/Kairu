using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Kairudev.Infrastructure.Persistence;

internal sealed class KairudevDbContextFactory : IDesignTimeDbContextFactory<KairudevDbContext>
{
    public KairudevDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("SQL_CONNECTION_STRING")
            ?? "Server=(localdb)\\mssqllocaldb;Database=kairudev;Trusted_Connection=true;";

        var builder = new DbContextOptionsBuilder<KairudevDbContext>();
        builder.UseSqlServer(connectionString);

        return new KairudevDbContext(builder.Options);
    }
}
