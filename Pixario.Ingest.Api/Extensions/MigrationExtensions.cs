using Microsoft.EntityFrameworkCore;
using Pixario.Ingest.Infrastructure.Persistence;

namespace Pixario.Ingest.Api.Extensions;

public static class MigrationExtensions
{
    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<IngestDbContext>();

        var pendingMigrations = await db.Database.GetPendingMigrationsAsync();

        if (pendingMigrations.Any())
        {
            await db.Database.MigrateAsync();
        }
    }
}