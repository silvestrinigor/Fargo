using Fargo.Realtime.Contracts;
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

    public static string FormatText(EntityChangedNotification notification)
        => string.Create(
            CultureInfo.InvariantCulture,
            $"{notification.OccurredAt.UtcDateTime:yyyy-MM-dd HH:mm:ss}Z  {notification.ChangeType,-8} {notification.EntityType,-8} {notification.Guid}");

    public static string FormatJson(EntityChangedNotification notification)
        => JsonSerializer.Serialize(notification, JsonOptions);
}
