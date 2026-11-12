using Fargo.Application.Abstractions;
using Fargo.Application.Commands.Initialization;
using Fargo.ServiceSeed.Options;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace Fargo.ServiceSeed.Services;

/// <summary>
/// Runs the system initialization command once at startup, then stops the host. A failure is recorded on
/// the tracing activity and rethrown.
/// </summary>
public sealed class FargoSeedService(
    IServiceProvider serviceProvider,
    IHostApplicationLifetime hostApplicationLifetime,
    ILogger<FargoSeedService> logger
) : BackgroundService
{
    public const string ActivitySourceName = "Seeds";

    private static readonly ActivitySource activitySource = new(ActivitySourceName);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var activity = activitySource.StartActivity("Seeding database", ActivityKind.Client);

        logger.LogInformation("Starting seed database.");

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

            logger.LogInformation("Seed database completed successfully.");
        }
        catch (Exception ex)
        {
            activity?.AddException(ex);

            logger.LogError(ex, "Seed database failed.");

            throw;
        }
        finally
        {
            logger.LogInformation("Stopping seed database.");

            hostApplicationLifetime.StopApplication();
        }
    }
}
