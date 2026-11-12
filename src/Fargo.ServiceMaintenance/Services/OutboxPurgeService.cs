using Fargo.Application.Abstractions;
using Fargo.Application.Commands.Outbox;
using Fargo.ServiceMaintenance.Options;
using Microsoft.Extensions.Options;

namespace Fargo.ServiceMaintenance.Services;

/// <summary>
/// Deletes published outbox messages older than <see cref="OutboxPurgeOptions.Retention"/>, once at startup and
/// then every <see cref="OutboxPurgeOptions.PurgeInterval"/>.
/// </summary>
/// <remarks>
/// It needs only the database, so it runs even when the broker is down or not used. Without a broker nothing is
/// published, so nothing becomes purgeable. A failed purge is logged and tried again at the next interval.
/// </remarks>
public sealed class OutboxPurgeService(
    IServiceScopeFactory scopeFactory,
    IOptions<OutboxPurgeOptions> options,
    TimeProvider timeProvider,
    ILogger<OutboxPurgeService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;

        using var timer = new PeriodicTimer(settings.PurgeInterval, timeProvider);

        try
        {
            do
            {
                await PurgeOnceAsync(settings, stoppingToken);
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Shutting down.
        }
    }

    internal async Task PurgeOnceAsync(OutboxPurgeOptions settings, CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();

            var purge = scope.ServiceProvider
                .GetRequiredService<ICommandHandler<PurgeProcessedOutboxCommand, int>>();

            await purge.HandleAsync(
                new PurgeProcessedOutboxCommand(timeProvider.GetUtcNow() - settings.Retention),
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.OutboxPurgeFailed(exception);
        }
    }
}