using Fargo.Cli.Commands;

var rootCommand = RootCommandFactory.Create();

return await rootCommand.Parse(args).InvokeAsync();
