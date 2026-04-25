using Fargo.Application.Authentication;
using Fargo.Application.Persistence;
using Fargo.Domain;
using Fargo.Domain.ApiClients;

namespace Fargo.Application.ApiClients;

public sealed record ApiClientCreateCommand(string Name, string? Description) : ICommand<ApiClientCreatedResult>;

public sealed record ApiClientCreatedResult(Guid Guid, string PlainKey);

public sealed class ApiClientCreateCommandHandler(
    ActorService actorService,
    IApiClientRepository apiClientRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser
) : ICommandHandler<ApiClientCreateCommand, ApiClientCreatedResult>
{
    public async Task<ApiClientCreatedResult> Handle(ApiClientCreateCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.CreateApiClient);

        var plainKey = ApiKeyGenerator.Generate();
        var keyHash = ApiKeyGenerator.Hash(plainKey);

        var client = new ApiClient
        {
            Name = new(command.Name),
            Description = command.Description is not null ? new(command.Description) : Description.Empty,
            KeyHash = keyHash
        };

        apiClientRepository.Add(client);

        await unitOfWork.SaveChanges(cancellationToken);

        return new ApiClientCreatedResult(client.Guid, plainKey);
    }
}
