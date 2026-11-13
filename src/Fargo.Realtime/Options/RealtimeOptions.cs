using System.ComponentModel.DataAnnotations;

namespace Fargo.Realtime.Options;

/// <summary>
/// Configuration of the real-time service, bound from the <c>Realtime</c> section.
/// </summary>
public sealed class RealtimeOptions
{
    public const string SectionName = "Realtime";

    /// <summary>
    /// How often the partition access of every connected actor is read again, so access changes and
    /// deactivations reach open connections.
    /// </summary>
    [Range(typeof(TimeSpan), "00:00:05", "01:00:00")]
    public TimeSpan AccessRefreshInterval { get; set; } = TimeSpan.FromMinutes(1);
}
