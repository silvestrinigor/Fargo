using Fargo.Application.Requests.Commands;
using System.Diagnostics;

namespace Fargo.SeedService;

/// <summary>
/// Background service responsible for executing the system seed process during application startup.
/// </summary>
/// <remarks>
/// This service creates a scoped dependency container, resolves the
/// <see cref="ICommandHandler{TCommand}"/> for <see cref="InitializeSystemCommand"/>,
/// and executes it to initialize the system state.
/// <para>
/// The default administrator configuration is no longer read directly by this service.
/// That configuration is provided through application options and consumed by the
/// <see cref="InitializeSystemCommandHandler"/>.
/// </para>
/// <para>
/// After the seed process finishes, the application is stopped through
/// <see cref="IHostApplicationLifetime.StopApplication"/>.
/// </para>
/// </remarks>
public sealed class SeedService(
        IServiceProvider serviceProvider,
        IHostApplicationLifetime hostApplicationLifetime
        ) : BackgroundService
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
                "Seeding database", ActivityKind.Client
                );

        try
        {
            using var scope = serviceProvider.CreateScope();

            var command = new InitializeSystemCommand();

            var handler = scope.ServiceProvider
                .GetRequiredService<ICommandHandler<InitializeSystemCommand>>();

            await handler.Handle(command, stoppingToken);
        }
        catch (Exception ex)
        {
            activity?.AddException(ex);
            throw;
        }

        hostApplicationLifetime.StopApplication();
    }
}