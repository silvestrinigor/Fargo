using Fargo.RealtimeContracts;
using System.Globalization;
using System.Text.Json;

namespace Fargo.Cli.Commands.Realtime;

/// <summary>
/// Formats pushed changes for the terminal: one line per change, either aligned
/// text or JSON (one object per line, for piping into other tools).
/// </summary>
internal static class RealtimeEventWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Formats a change as one line, for example
    /// <c>2026-11-06 14:03:22Z  edited   article  a3e9d1c2-…</c>.
    /// </summary>
    public static string FormatText(EntityChangedNotification notification)
        => string.Create(
            CultureInfo.InvariantCulture,
            $"{notification.OccurredAt.UtcDateTime:yyyy-MM-dd HH:mm:ss}Z  {notification.ChangeType,-8} {notification.EntityType,-8} {notification.Guid}");

    /// <summary>
    /// Formats a change as a single-line JSON object with the wire field names.
    /// </summary>
    public static string FormatJson(EntityChangedNotification notification)
        => JsonSerializer.Serialize(notification, JsonOptions);
}
