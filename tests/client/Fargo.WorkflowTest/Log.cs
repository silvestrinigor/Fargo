namespace Fargo.WorkflowTest;

/// <summary>
/// Minimal colourised console logger for the workflow driver. Keeps the output
/// readable as a narrated sequence of steps.
/// </summary>
internal static class Log
{
    private static int stepNumber;

    public static void Step(string message)
    {
        stepNumber++;
        Write(ConsoleColor.Cyan, $"\n[{stepNumber:D2}] {message}");
    }

    public static void Info(string message) => Write(ConsoleColor.Gray, $"     {message}");

    public static void Success(string message) => Write(ConsoleColor.Green, $"     \u2713 {message}");

    public static void Warn(string message) => Write(ConsoleColor.Yellow, $"     ! {message}");

    public static void Error(string message) => Write(ConsoleColor.Red, $"     \u2717 {message}");

    public static void Banner(string message)
    {
        var line = new string('=', Math.Max(message.Length, 40));
        Write(ConsoleColor.White, $"\n{line}\n{message}\n{line}");
    }

    private static void Write(ConsoleColor color, string message)
    {
        var previous = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ForegroundColor = previous;
    }
}
