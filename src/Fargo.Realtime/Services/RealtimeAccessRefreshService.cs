using Fargo.Core.Entities.Actors;
using Fargo.Realtime.Connections;
using Fargo.Realtime.Options;
using Microsoft.Extensions.Options;

namespace Fargo.Realtime.Services;

/// <summary>
/// Reads again, every <see cref="RealtimeOptions.AccessRefreshInterval"/>, the
/// partition access of each actor with an open connection.
/// </summary>
/// <remarks>
/// Access is otherwise resolved only when a connection opens. An access change reaches open connections
/// within one interval, and the connections of an actor that was deleted or deactivated are closed.
/// Each actor is resolved once per pass, however many connections it has.
/// </remarks>
public sealed class RealtimeAccessRefreshService(
    IServiceScopeFactory scopeFactory,
    RealtimeConnectionRegistry registry,
    IOptions<RealtimeOptions> options,
    TimeProvider timeProvider,
    ILogger<RealtimeAccessRefreshService> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(options.Value.AccessRefreshInterval, timeProvider);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    await using var scope = scopeFactory.CreateAsyncScope();

                    var actorResolver = scope.ServiceProvider.GetRequiredService<ActorResolver>();

                    await RefreshAsync(
                        registry,
                        async (actorGuid, actorType, cancellationToken) =>
                            (await actorResolver.GetActorByGuidAndTypeAsync(actorGuid, actorType, cancellationToken))
                                ?.PartitionAccessGuids,
                        stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception exception)
                {
                    // For example the database is unreachable: keep the last
                    // known access and try again on the next tick.
                    logger.AccessRefreshFailed(exception);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Stopping.
        }
    }

    /// <summary>
    /// <paramref name="resolveAccess"/> returns <see langword="null"/> when the actor no longer exists or is
    /// inactive; its connections are then aborted.
    /// </summary>
    internal static async Task RefreshAsync(
        RealtimeConnectionRegistry registry,
        Func<Guid, ActorType, CancellationToken, Task<IReadOnlySet<Guid>?>> resolveAccess,
        CancellationToken cancellationToken)
    {
        var byActor = registry.GetAll().GroupBy(connection => (connection.ActorGuid, connection.ActorType));

        foreach (var actorConnections in byActor)
        {
            var (actorGuid, actorType) = actorConnections.Key;

            var access = await resolveAccess(actorGuid, actorType, cancellationToken);

            foreach (var connection in actorConnections)
            {
                if (access is null)
                {
                    connection.Abort();
                }
                else
                {
                    connection.UpdateAccess(access);
                }
            }
        }
    }
}
