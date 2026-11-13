using System.CommandLine;

namespace Fargo.Cli.Commands;

internal static class CommandGroups
{
    /// <summary>
    /// Reports whether the invoked command is one of <paramref name="groupNames"/> or nested under one.
    /// </summary>
    public static bool IsIn(ParseResult result, params string[] groupNames)
    {
        Command? command = result.CommandResult.Command;

        while (command is not null)
        {
            if (groupNames.Contains(command.Name, StringComparer.Ordinal))
            {
                return true;
            }

            command = command.Parents.OfType<Command>().FirstOrDefault();
        }

        return false;
    }
}
