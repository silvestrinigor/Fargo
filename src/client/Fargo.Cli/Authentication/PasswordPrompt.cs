namespace Fargo.Cli.Authentication;

/// <summary>
/// Reads a password from the console without echoing the characters, so the
/// secret is not visible on screen or captured in shell history.
/// </summary>
public static class PasswordPrompt
{
    /// <summary>
    /// Prompts for a password on the standard error stream and reads it with
    /// masking. When input is redirected (non-interactive), a plain line is read
    /// instead so the command still works in scripts and pipes.
    /// </summary>
    /// <param name="prompt">The text shown before the masked input.</param>
    /// <returns>The entered password, or an empty string if nothing was typed.</returns>
    public static string Read(string prompt = "Password: ")
    {
        Console.Error.Write(prompt);

        // When stdin is redirected there is no interactive console to mask, so
        // fall back to reading a full line.
        if (Console.IsInputRedirected)
        {
            var line = Console.ReadLine() ?? string.Empty;

            return line;
        }

        var password = new System.Text.StringBuilder();

        while (true)
        {
            var keyInfo = Console.ReadKey(intercept: true);

            if (keyInfo.Key == ConsoleKey.Enter)
            {
                Console.Error.WriteLine();
                break;
            }

            if (keyInfo.Key == ConsoleKey.Backspace)
            {
                if (password.Length > 0)
                {
                    password.Remove(password.Length - 1, 1);
                    Console.Error.Write("\b \b");
                }

                continue;
            }

            // Ignore other control characters (arrows, function keys, etc.).
            if (char.IsControl(keyInfo.KeyChar))
            {
                continue;
            }

            password.Append(keyInfo.KeyChar);
            Console.Error.Write('*');
        }

        return password.ToString();
    }
}
