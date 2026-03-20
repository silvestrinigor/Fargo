using Fargo.Application.Models.UserModels;
using Fargo.Domain.ValueObjects;
using Fargo.HttpApi.Client.Contracts;

namespace Fargo.Web.Features.Users;

public sealed class UserApi(IUserClient userClient)
{
    public async Task<IReadOnlyList<UserInformation>> GetManyAsync(
        CancellationToken cancellationToken = default)
    {
        var users = await userClient.GetManyAsync(cancellationToken: cancellationToken);
        return [.. users];
    }

    public Task<UserInformation?> GetSingleAsync(
        Guid userGuid,
        CancellationToken cancellationToken = default) =>
        userClient.GetSingleAsync(userGuid, cancellationToken: cancellationToken);

    public async Task CreateAsync(
        UserCreateModel model,
        CancellationToken cancellationToken = default)
    {
        await userClient.CreateAsync(model, cancellationToken);
    }

    public Task UpdateAsync(
        Guid userGuid,
        UserUpdateModel model,
        CancellationToken cancellationToken = default) =>
        userClient.UpdateAsync(userGuid, model, cancellationToken);

    public Task DeleteAsync(
        Guid userGuid,
        CancellationToken cancellationToken = default) =>
        userClient.DeleteAsync(userGuid, cancellationToken);
}
