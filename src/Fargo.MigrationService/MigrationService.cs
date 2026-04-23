using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Fargo.MigrationService;

/// <summary>
/// Background service responsible for applying pending database migrations
/// during application startup.
/// </summary>
/// <remarks>
/// This service creates a scoped service provider, resolves the
/// <see cref="FargoDbContext"/>, and executes the migration process.
/// <para>
/// After the migration finishes, the application is stopped through
/// <see cref="IHostApplicationLifetime.StopApplication"/>.
/// </para>
/// </remarks>
public sealed class MigrationService(
    IServiceProvider serviceProvider,
    IHostApplicationLifetime hostApplicationLifetime
    ) : BackgroundService
{
    /// <summary>
    /// Gets the name of the <see cref="ActivitySource"/> used for tracing migration operations.
    /// </summary>
    public const string ActivitySourceName = "Migrations";

    /// <summary>
    /// Activity source used to emit tracing information for migration execution.
    /// </summary>
    private static readonly ActivitySource activitySource = new(ActivitySourceName);

    /// <summary>
    /// Executes the background migration process.
    /// </summary>
    /// <param name="stoppingToken">
    /// A token that indicates when the background operation should be cancelled.
    /// </param>
    /// <returns>
    /// A task that represents the lifetime of the background execution operation.
    /// </returns>
    /// <remarks>
    /// This method starts a tracing activity, creates a service scope,
    /// resolves the write database context, and applies pending migrations.
    /// <para>
    /// If an exception occurs during execution, it is attached to the current activity
    /// and then rethrown.
    /// </para>
    /// <para>
    /// When the process completes, the host application is instructed to stop.
    /// </para>
    /// </remarks>
    /// <exception cref="Exception">
    /// Thrown when an unexpected error occurs during the migration execution.
    /// </exception>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var activity = activitySource.StartActivity(
            "Migrating database", ActivityKind.Client
        );

        try
        {
            using var scope = serviceProvider.CreateScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<FargoDbContext>();

            await RunMigrationAsync(dbContext, stoppingToken);
        }
        catch (Exception ex)
        {
            activity?.AddException(ex);
            throw;
        }

        hostApplicationLifetime.StopApplication();
    }

    /// <summary>
    /// Applies pending migrations for the specified database context.
    /// </summary>
    /// <param name="dbContext">
    /// The write database context used to access and migrate the database.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the migration operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous migration operation.
    /// </returns>
    /// <remarks>
    /// The migration is executed through the Entity Framework execution strategy
    /// so transient failures can be retried according to the configured provider behavior.
    /// </remarks>
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
