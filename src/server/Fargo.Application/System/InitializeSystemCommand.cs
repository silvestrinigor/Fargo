using Fargo.Application.Common;
using Fargo.Core.Informations;
using Fargo.Core.Security;

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
