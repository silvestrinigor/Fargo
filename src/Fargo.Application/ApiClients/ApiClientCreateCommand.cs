using Fargo.Application.Authentication;
using Fargo.Application.Persistence;
using Fargo.Domain;
using Fargo.Domain.ClientApplications;

namespace Fargo.Application.ApiClients;

/// <summary>Command to create a new API client and generate its initial API key.</summary>
/// <param name="Name">The display name for the API client.</param>
/// <param name="Description">An optional description.</param>
public sealed record ApiClientCreateCommand(string Name, string? Description) : ICommand<ApiClientCreatedResult>;

/// <summary>Result returned after successfully creating an API client.</summary>
/// <param name="Guid">The unique identifier assigned to the new API client.</param>
/// <param name="PlainKey">The plain-text API key, shown once at creation time.</param>
public sealed record ApiClientCreatedResult(Guid Guid, string PlainKey);

/// <summary>Handles <see cref="ApiClientCreateCommand"/>.</summary>
public sealed class ApiClientCreateCommandHandler(
    ActorService actorService,
    IApiClientRepository apiClientRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser
) : ICommandHandler<ApiClientCreateCommand, ApiClientCreatedResult>
{
    /// <summary>Creates the API client, hashes its key, and persists the entity.</summary>
    /// <param name="command">The command containing the API client name and description.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    public async Task<ApiClientCreatedResult> Handle(ApiClientCreateCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.CreateApiClient);

        var plainKey = ApiKeyGenerator.Generate();
        var keyHash = ApiKeyGenerator.Hash(plainKey);

        var client = new ClientApplication
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
