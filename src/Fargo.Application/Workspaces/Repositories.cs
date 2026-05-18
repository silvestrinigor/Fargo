using Fargo.Core.Workspaces;

namespace Fargo.Application.Workspaces;

public interface IWorkspaceRepository
{
    void Add(Workspace workspace);

    Task<Workspace?> GetByGuid(Guid workspaceGuid, CancellationToken cancellationToken = default);

    Task<Workspace> GetFoundByGuid(Guid workspaceGuid, CancellationToken cancellationToken = default);

    Task<int> GetNextSequence(Guid workspaceGuid, CancellationToken cancellationToken = default);
}

public sealed class WorkspaceNotFoundFargoApplicationException(Guid workspaceGuid)
    : FargoApplicationException($"Workspace '{workspaceGuid}' was not found.")
{
    public Guid WorkspaceGuid { get; } = workspaceGuid;
}
