using Fargo.Application.Common;

namespace Fargo.Application.ServiceAccounts;

/// <summary>Command to delete a service account.</summary>
public sealed record ServiceAccountDeleteCommand(
    Guid ServiceAccountGuid
) : ICommand;