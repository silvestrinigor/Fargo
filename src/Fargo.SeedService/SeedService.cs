using Fargo.Application.Requests.Commands;
using Fargo.Domain.ValueObjects;
using Fargo.SeedService.Extensions;
using System.Diagnostics;

namespace Fargo.SeedService;

/// <summary>
/// Background service responsible for executing the system seed process during application startup.
/// </summary>
/// <remarks>
/// This service creates a scoped dependency container, reads the default administrator
/// configuration values from <see cref="IConfiguration"/>, resolves the
/// <see cref="ICommandHandler{TCommand}"/> for <see cref="InitializeSystemCommand"/>,
/// and executes it to initialize the system state.
/// <para>
/// If the default administrator configuration values are available, they are converted
/// into <see cref="Nameid"/> and <see cref="Password"/> value objects and passed to the
/// initialization command.
/// </para>
/// <para>
/// After the seed process finishes, the application is stopped through
/// <see cref="IHostApplicationLifetime.StopApplication"/>.
/// </para>
/// </remarks>
public class SeedService(
        IServiceProvider serviceProvider,
        IHostApplicationLifetime hostApplicationLifetime,
        IConfiguration configuration
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
    /// reads the default administrator credentials from configuration,
    /// creates an <see cref="InitializeSystemCommand"/>, resolves its handler,
    /// and executes the command.
    /// <para>
    /// The configuration values are optional. When present, they are converted into
    /// <see cref="Nameid"/> and <see cref="Password"/> instances before being passed
    /// to the command.
    /// </para>
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

            var newUserName = configuration.GetApplicationConfiguration("DefaultAdminNameid");

            var newUserPass = configuration.GetApplicationConfiguration("DefaultAdminPassword");

            var command = new InitializeSystemCommand(
                    newUserName is not null ? new Nameid(newUserName) : null,
                    newUserPass is not null ? new Password(newUserPass) : null
                    );

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