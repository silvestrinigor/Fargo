using Fargo.Cli.Commands;

namespace Fargo.Cli;

public static class Application
{
    public static async Task<int> RunAsync()
    {
        var root = RootCommandFactory.Create();

        Console.WriteLine("Fargo Cli");
        Console.WriteLine("Type 'help' for help.");
        Console.WriteLine();

        while (true)
        {
            Console.Write("fargo> ");

            var input = Console.ReadLine();

            if (input is null)
            {
                break;
            }

            if (string.IsNullOrWhiteSpace(input))
            {
                continue;
            }

            var result = root.Parse(input);

            await result.InvokeAsync();
        }

        return 0;
    }
}
