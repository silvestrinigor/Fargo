using Fargo.Application.Interfaces.Solicitations.Commands;

namespace Fargo.Application.Solicitations.Commands.ContainerCommands.CreateContainer
{
    public sealed record CreateContainerCommand(string? Name, string? Description) : ICommand<CreateContainerCommand, Task>;
}
