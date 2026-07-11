using Fargo.Application;
using Fargo.Application.System;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace Fargo.ServiceSeed;

/// <summary>
/// Background service responsible for executing the system seed process during application startup.
/// </summary>
public sealed class FargoSeedService(
    IServiceProvider serviceProvider,
    IHostApplicationLifetime hostApplicationLifetime,
    ILogger<FargoSeedService> logger) : BackgroundService
{
    /// <summary>
    /// Gets the name of the <see cref="ActivitySource"/> used for tracing seed operations.
    /// </summary>
    public const string ActivitySourceName = "Seeds";

    /// <summary>
    /// Activity source used to emit tracing information for the seed execution.
    /// </summary>
    private static readonly ActivitySource activitySource = new(ActivitySourceName);

    /// <summary>
    /// Executes the background seed process.
    /// </summary>
    /// <param name="stoppingToken">
    /// A token that indicates when the background operation should be cancelled.
    /// </param>
    /// <returns>
    /// A task that represents the lifetime of the background execution operation.
    /// </returns>
    /// <remarks>
    /// This method starts a tracing activity, creates a service scope,
    /// resolves the handler for <see cref="InitializeSystemCommand"/>,
    /// and executes the command.
    /// <para>
    /// If an exception occurs during execution, it is attached to the current activity
    /// and then rethrown.
    /// </para>
    /// <para>
    /// When the process completes, the host application is instructed to stop.
    /// </para>
    /// </remarks>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var activity = activitySource.StartActivity(
            "Seeding database", ActivityKind.Client);

        logger.LogInformation("Starting system initialization.");

        try
        {
            using var scope = serviceProvider.CreateScope();

            var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<InitializeSystemCommand>>();

            var administratorsOptions = scope.ServiceProvider.GetRequiredService<IOptions<AdministratorsUserGroupOptions>>();

            var adminOptions = scope.ServiceProvider.GetRequiredService<IOptions<DefaultAdminOptions>>();

            var globalPartitionOptions = scope.ServiceProvider.GetRequiredService<IOptions<GlobalPartitionOptions>>();

            var command = new InitializeSystemCommand(
                new(adminOptions.Value.Nameid), new(adminOptions.Value.Password), new(adminOptions.Value.Description),
                new(administratorsOptions.Value.Nameid), new(administratorsOptions.Value.Description),
                new(globalPartitionOptions.Value.Name), new(globalPartitionOptions.Value.Description)
            );

            await handler.HandleAsync(command, stoppingToken);

            logger.LogInformation("System initialization completed successfully.");
        }
        catch (Exception ex)
        {
            activity?.AddException(ex);

            logger.LogError(ex, "System initialization failed.");

            throw;
        }
        finally
        {
            logger.LogInformation("Stopping seed application.");

            hostApplicationLifetime.StopApplication();
        }
    }
}
