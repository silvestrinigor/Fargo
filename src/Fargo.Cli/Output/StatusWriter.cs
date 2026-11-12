namespace Fargo.Cli.Output;

/// <summary>
/// Writes consistent, human-friendly status messages for write operations
/// (create/update/delete) and other confirmations. Plain ASCII, no color.
/// </summary>
public static class StatusWriter
{
    /// <summary>
    /// Writes a success line, optionally followed by an aligned key/value block
    /// of related details (for example the identifier of a created resource).
    /// </summary>
    public static void Success(string message, params (string Label, string? Value)[] details)
    {
        Console.WriteLine($"[OK] {message}");

        if (details.Length == 0)
        {
            return;
        }

        var labelWidth = details.Max(d => d.Label.Length);

        foreach (var (label, value) in details)
        {
            var text = string.IsNullOrEmpty(value) ? "-" : value;
            Console.WriteLine($"  {label.PadRight(labelWidth)}  {text}");
        }
    }

    /// <summary>Writes an informational line.</summary>
    public static void Info(string message)
    {
        Console.WriteLine(message);
    }
}
