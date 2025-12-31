namespace Fargo.Application.Solicitations.Commands.ContainerCommands
{
    public sealed record ContainerItemAddCommand(Guid EntityGuid, Guid ContainerGuid);
}
