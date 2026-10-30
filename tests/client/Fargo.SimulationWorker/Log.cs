namespace Fargo.SimulationWorker;

/// <summary>
/// Minimal colourised, thread-safe console logger for the simulation worker.
/// Lines are tagged with the operator (or "sim") that produced them so the
/// interleaved output of many concurrent operators stays readable.
/// </summary>
internal static class Log
{
    private static readonly Lock Gate = new();

    public static void Banner(string message)
    {
        var line = new string('=', Math.Max(message.Length, 40));
        Write(ConsoleColor.White, $"\n{line}\n{message}\n{line}");
    }

    public static void Info(string who, string message) => Write(ConsoleColor.Gray, Format(who, message));

    public static void Success(string who, string message) => Write(ConsoleColor.Green, Format(who, "\u2713 " + message));

    public static void Warn(string who, string message) => Write(ConsoleColor.Yellow, Format(who, "! " + message));

    public static void Error(string who, string message) => Write(ConsoleColor.Red, Format(who, "\u2717 " + message));

    public static void Action(string who, string message) => Write(ConsoleColor.DarkGray, Format(who, message));

    public static void Metrics(string message) => Write(ConsoleColor.Cyan, message);

    private static string Format(string who, string message) =>
        $"[{DateTime.Now:HH:mm:ss}] {who,-12} {message}";

    private static void Write(ConsoleColor color, string message)
    {
        lock (Gate)
        {
            var previous = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = previous;
        }
    }
}
