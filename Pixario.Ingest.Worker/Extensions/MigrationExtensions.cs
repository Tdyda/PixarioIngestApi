using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Pixario.Ingest.Infrastructure.Persistence;

namespace Pixario.Ingest.Worker.Extensions;

public static class MigrationExtensions
{
    public static async Task ApplyMigrationsAsync<TContext>(
        this IServiceProvider services)
        where TContext : DbContext
    {
        using var scope = services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<TContext>();

        var pendingMigrations = await db.Database.GetPendingMigrationsAsync();

        if (pendingMigrations.Any())
        {
            await db.Database.MigrateAsync();
        }
    }
}