using System.ComponentModel.DataAnnotations;

namespace Fargo.Maintenance.Options;

public sealed class OutboxPurgeOptions
{
    public const string SectionName = "Maintenance:Outbox";

    /// <summary>
    /// How long a published message is kept before it is deleted. Messages not yet published are never deleted.
    /// </summary>
    [Range(typeof(TimeSpan), "00:01:00", "3650.00:00:00")]
    public TimeSpan Retention { get; set; } = TimeSpan.FromDays(7);

    [Range(typeof(TimeSpan), "00:00:01", "1.00:00:00")]
    public TimeSpan PurgeInterval { get; set; } = TimeSpan.FromHours(1);
}
