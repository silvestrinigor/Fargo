using Fargo.Application.Abstractions;
using Fargo.Application.Commands.Outbox;
using Fargo.Application.Dtos.Outbox;
using Fargo.ServiceOutbox.Options;
using Microsoft.Extensions.Options;

namespace Fargo.ServiceOutbox.Services;

/// <summary>
/// Owns only scheduling: a full batch is followed immediately by the next one; an empty or partial batch waits
/// <see cref="OutboxOptions.PollInterval"/>. After a failure the wait doubles, up to
/// <see cref="OutboxOptions.MaxRetryDelay"/>, and resets on the next success. Published messages past their
/// retention are purged every <see cref="OutboxOptions.PurgeInterval"/>. The publishing and purging rules live
/// in the application command handlers.
/// </summary>
public sealed class OutboxRelayService(
    IServiceScopeFactory scopeFactory,
    IOptions<OutboxOptions> options,
    TimeProvider timeProvider,
    ILogger<OutboxRelayService> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;
        var consecutiveFailures = 0;
        var nextPurgeAt = timeProvider.GetUtcNow();

        while (!stoppingToken.IsCancellationRequested)
        {
            TimeSpan delay;

            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();

                var publish = scope.ServiceProvider
                .GetRequiredService<ICommandHandler<PublishOutboxBatchCommand, OutboxBatchResultDto>>();

                var result = await publish.HandleAsync(new PublishOutboxBatchCommand(settings.BatchSize), stoppingToken);

                if (result.Failed)
                {
                    consecutiveFailures++;
                    delay = GetRetryDelay(settings, consecutiveFailures);
                }
                else
                {
                    consecutiveFailures = 0;
                    delay = result.Claimed >= settings.BatchSize ? TimeSpan.Zero : settings.PollInterval;
                }

                var now = timeProvider.GetUtcNow();

                if (now >= nextPurgeAt)
                {
                    var purge = scope.ServiceProvider
                        .GetRequiredService<ICommandHandler<PurgeProcessedOutboxCommand, int>>();

                    await purge.HandleAsync(new PurgeProcessedOutboxCommand(now - settings.Retention), stoppingToken);

                    nextPurgeAt = now + settings.PurgeInterval;
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                // For example the database is unreachable. Keep running.
                consecutiveFailures++;
                delay = GetRetryDelay(settings, consecutiveFailures);

                logger.OutboxRelayFailed(exception, delay);
            }

            if (delay <= TimeSpan.Zero)
            {
                continue;
            }

            try
            {
                await Task.Delay(delay, timeProvider, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    internal static TimeSpan GetRetryDelay(OutboxOptions settings, int consecutiveFailures)
    {
        var exponent = Math.Min(consecutiveFailures - 1, 30);
        var ticks = settings.PollInterval.Ticks * Math.Pow(2, exponent);

        return ticks >= settings.MaxRetryDelay.Ticks
            ? settings.MaxRetryDelay
            : TimeSpan.FromTicks((long)ticks);
    }
}
