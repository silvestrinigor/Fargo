using Fargo.Application.Abstractions;
using Fargo.Application.Commands.Idempotency;
using Fargo.ServiceMaintenance.Options;
using Microsoft.Extensions.Options;

namespace Fargo.ServiceMaintenance.Services;

/// <summary>
/// Deletes expired idempotency records, once at startup and then every
/// <see cref="IdempotencyKeyPurgeOptions.PurgeInterval"/>.
/// </summary>
/// <remarks>
/// A failed purge is logged and tried again at the next interval, because expired records only waste space:
/// the API already ignores them. Several instances can run at once, since deleting a record twice is harmless.
/// </remarks>
public sealed class IdempotencyKeyPurgeService(
    IServiceScopeFactory scopeFactory,
    IOptions<IdempotencyKeyPurgeOptions> options,
    TimeProvider timeProvider,
    ILogger<IdempotencyKeyPurgeService> logger) : BackgroundService
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

    internal async Task PurgeOnceAsync(IdempotencyKeyPurgeOptions settings, CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();

            var purge = scope.ServiceProvider
                .GetRequiredService<ICommandHandler<PurgeExpiredIdempotencyKeysCommand, int>>();

            await purge.HandleAsync(
                new PurgeExpiredIdempotencyKeysCommand(timeProvider.GetUtcNow(), settings.BatchSize),
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.IdempotencyKeyPurgeFailed(exception);
        }
    }
}