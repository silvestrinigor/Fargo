using Fargo.Application.Common;

namespace Fargo.Application.ServiceAccounts;

/// <summary>Command to update a service account's mutable state.</summary>
public sealed record ServiceAccountUpdateCommand(
    Guid ServiceAccountGuid,
    ServiceAccountUpdateDto Update
) : ICommand;