using Fargo.Application.Workspaces;
using Fargo.Core.Workspaces;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Repositories;

public sealed class WorkspaceRepository(FargoDbContext context) : IWorkspaceRepository
{
    public void Add(Workspace workspace)
        => context.Workspaces.Add(workspace);

    public async Task<Workspace?> GetByGuid(
        Guid workspaceGuid,
        CancellationToken cancellationToken = default)
        => await context.Workspaces
            .Include(static x => x.Commands)
            .SingleOrDefaultAsync(x => x.Guid == workspaceGuid, cancellationToken);

    public async Task<Workspace> GetFoundByGuid(
        Guid workspaceGuid,
        CancellationToken cancellationToken = default)
        => await GetByGuid(workspaceGuid, cancellationToken)
            ?? throw new WorkspaceNotFoundFargoApplicationException(workspaceGuid);

    public async Task<int> GetNextSequence(
        Guid workspaceGuid,
        CancellationToken cancellationToken = default)
    {
        var lastSequence = await context.WorkspaceCommands
            .Where(x => x.WorkspaceGuid == workspaceGuid)
            .MaxAsync(x => (int?)x.Sequence, cancellationToken);

        return (lastSequence ?? 0) + 1;
    }
}
