using System.Collections.Concurrent;

namespace Fargo.SimulationWorker;

internal sealed class Metrics
{
    private readonly ConcurrentDictionary<string, long> _actions = new();
    private readonly DateTimeOffset _startedAt = DateTimeOffset.UtcNow;

    private long _totalOps;
    private long _errors;
    private long _rateLimited;

    public long TotalOps => Interlocked.Read(ref _totalOps);

    public long Errors => Interlocked.Read(ref _errors);

    public long RateLimited => Interlocked.Read(ref _rateLimited);

    public void RecordAction(string action)
    {
        Interlocked.Increment(ref _totalOps);
        _actions.AddOrUpdate(action, 1, (_, v) => v + 1);
    }

    public void RecordError() => Interlocked.Increment(ref _errors);

    public void RecordRateLimited() => Interlocked.Increment(ref _rateLimited);

    public string Summarize()
    {
        var elapsed = DateTimeOffset.UtcNow - _startedAt;
        var minutes = Math.Max(elapsed.TotalMinutes, 1.0 / 60.0);
        var opsPerMin = TotalOps / minutes;

        var top = _actions
            .OrderByDescending(kv => kv.Value)
            .Select(kv => $"{kv.Key}={kv.Value}");

        return
            $"elapsed {elapsed:hh\\:mm\\:ss} | ops {TotalOps} ({opsPerMin:F1}/min) | " +
            $"errors {Errors} | 429s {RateLimited}\n" +
            $"             actions: {string.Join(", ", top)}";
    }
}
