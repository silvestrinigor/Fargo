using Fargo.Core.Shared;

namespace Fargo.Application.System;

public sealed record InitializeSystemCommand(
    Nameid UserAdminNameid,
    Password UserAdminPassword,
    Description UserAdminDescription,
    Nameid UserGroupAdministratorsNameid,
    Description UserGroupAdministratorsDescription,
    Name GlobalPartitionName,
    Description GlobalPartitionDescription
) : ICommand;
