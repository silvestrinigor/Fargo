namespace Fargo.Cli.Output;

/// <summary>
/// Writes status messages for write operations and other confirmations. Plain ASCII, no color.
/// </summary>
public static class StatusWriter
{
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

    public static void Info(string message)
    {
        Console.WriteLine(message);
    }
}
