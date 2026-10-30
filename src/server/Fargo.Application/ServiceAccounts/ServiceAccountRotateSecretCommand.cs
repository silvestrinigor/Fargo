using Fargo.Application.Common;

namespace Fargo.Application.ServiceAccounts;

/// <summary>Command to rotate a service account's secret. Returns the new one-time secret.</summary>
public sealed record ServiceAccountRotateSecretCommand(
    Guid ServiceAccountGuid
) : ICommand<ServiceAccountCreatedDto>;