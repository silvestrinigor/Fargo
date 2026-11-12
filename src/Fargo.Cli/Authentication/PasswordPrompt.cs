namespace Fargo.Cli.Authentication;

/// <summary>
/// Reads a password from the console without echoing the typed characters.
/// </summary>
public static class PasswordPrompt
{
    /// <summary>
    /// Writes the prompt to standard error so redirected output stays clean. When input is
    /// redirected, a plain line is read instead so the command still works in scripts and pipes.
    /// </summary>
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
