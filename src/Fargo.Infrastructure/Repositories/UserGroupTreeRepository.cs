using Fargo.Application.Models.TreeModels;
using Fargo.Application.Repositories;
using Fargo.Domain.Entities;
using Fargo.Domain.ValueObjects;
using Fargo.Infrastructure.Extensions;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Repositories;

public sealed class UserGroupTreeRepository(FargoDbContext dbContext) : IUserGroupTreeRepository
{
    private readonly DbSet<UserGroup> userGroups = dbContext.UserGroups;
    private readonly DbSet<User> users = dbContext.Users;

    public async Task<IReadOnlyCollection<TreeNode>> GetMembers(
        Pagination pagination,
        IReadOnlyCollection<Guid> accessiblePartitionGuids,
        Guid? userGroupGuid,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
