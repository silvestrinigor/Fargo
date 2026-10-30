using Fargo.Application.Common;

namespace Fargo.Application.ServiceAccounts;

/// <summary>Command to create a service account. Returns its id and one-time secret.</summary>
public sealed record ServiceAccountCreateCommand(
    ServiceAccountCreateDto Create
) : ICommand<ServiceAccountCreatedDto>;
