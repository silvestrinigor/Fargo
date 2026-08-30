using Fargo.Cli;

var root = Application.Create();

await root.Parse(args).InvokeAsync();
