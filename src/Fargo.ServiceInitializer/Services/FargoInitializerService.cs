using Fargo.Application.Abstractions;
using Fargo.Application.Commands.Initialization;
using Fargo.ServiceInitializer.Options;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace Fargo.ServiceInitializer.Services;

/// <summary>
/// Runs the system initialization command once at startup, then stops the host. A failure is recorded on
/// the tracing activity and rethrown.
/// </summary>
public sealed class FargoInitializerService(
    IServiceProvider serviceProvider,
    IHostApplicationLifetime hostApplicationLifetime,
    ILogger<FargoInitializerService> logger
) : BackgroundService
{
    public const string ActivitySourceName = "Initializer";

    private static readonly ActivitySource activitySource = new(ActivitySourceName);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var activity = activitySource.StartActivity("Initializing system", ActivityKind.Client);

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
            logger.LogInformation("Stopping system initialization.");

            hostApplicationLifetime.StopApplication();
        }
    }
}
