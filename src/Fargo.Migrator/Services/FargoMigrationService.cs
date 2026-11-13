using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Fargo.Migrator.Services;

/// <summary>
/// Applies pending database migrations once at startup, then stops the host. A failure is recorded on the
/// tracing activity and rethrown.
/// </summary>
public sealed class FargoMigrationService(
    IServiceProvider serviceProvider,
    IHostApplicationLifetime hostApplicationLifetime,
    ILogger<FargoMigrationService> logger) : BackgroundService
{
    public const string ActivitySourceName = "Migrations";

    private static readonly ActivitySource activitySource = new(ActivitySourceName);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var activity = activitySource.StartActivity("Migrating database", ActivityKind.Client);

        logger.LogInformation("Starting database migration.");

        try
        {
            using var scope = serviceProvider.CreateScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<FargoDbContext>();

            await RunMigrationAsync(dbContext, stoppingToken);

            logger.LogInformation("Database migration completed successfully.");
        }
        catch (Exception ex)
        {
            activity?.AddException(ex);

            logger.LogError(ex, "Database migration failed.");

            throw;
        }
        finally
        {
            logger.LogInformation("Stopping database migration.");

            hostApplicationLifetime.StopApplication();
        }
    }

    // Run through the execution strategy so transient failures are retried.
    private static async Task RunMigrationAsync(
        FargoDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
        });
    }
}
