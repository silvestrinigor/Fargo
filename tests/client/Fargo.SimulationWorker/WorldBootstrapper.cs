using Fargo.ClientHttp;
using Fargo.ClientHttp.Models;

namespace Fargo.SimulationWorker;

/// <summary>
/// Performs the one-time, admin-driven setup the simulation needs before the
/// operator loops start: a small partition tree, per-role user groups carrying
/// the permissions and partition access each role requires, and one user per
/// simulated operator. New users are created with the mandatory first-login
/// password-change flag, which this bootstrapper clears so the operators can
/// authenticate immediately.
/// </summary>
internal sealed class WorldBootstrapper(
    FargoApiClient admin,
    FargoClientBuilder clientBuilder,
    SimulationOptions options,
    Names names,
    Check check)
{
    private readonly FargoApiClient _admin = admin;
    private readonly FargoClientBuilder _clientBuilder = clientBuilder;
    private readonly SimulationOptions _options = options;
    private readonly Names _names = names;
    private readonly Check _check = check;

    public async Task<SimulationWorld> BootstrapAsync(CancellationToken cancellationToken)
    {
        Log.Banner("SIMULATION SETUP");

        var partitions = await CreatePartitionsAsync(cancellationToken);

        // One group per role, each granted the permissions that role exercises
        // and access to the whole set of simulation partitions (so any operator
        // can act in any partition the simulation uses).
        var groupByRole = await CreateRoleGroupsAsync(partitions, cancellationToken);

        var operators = await CreateOperatorsAsync(partitions, groupByRole, cancellationToken);

        Log.Success("setup", $"World ready: {partitions.Count} partitions, {operators.Count} operators.");
        return new SimulationWorld(partitions, operators);
    }

    private async Task<List<Guid>> CreatePartitionsAsync(CancellationToken cancellationToken)
    {
        Log.Info("setup", "Creating a small partition tree (Site > Zone-1..3).");

        var site = await _admin.V1.Partitions.PostAsync(
            new PartitionCreateDto
            {
                Name = _names.Display("sim-site"),
                Description = "Simulation top-level site partition.",
            },
            cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Failed to create site partition.");

        var zones = new List<Guid> { site };
        for (var z = 1; z <= 3; z++)
        {
            var zone = await _admin.V1.Partitions.PostAsync(
                new PartitionCreateDto
                {
                    Name = _names.Display($"sim-zone-{z}"),
                    ParentPartitionGuid = site,
                },
                cancellationToken: cancellationToken)
                ?? throw new InvalidOperationException($"Failed to create zone {z} partition.");
            zones.Add(zone);
        }

        return zones;
    }

    private async Task<Dictionary<OperatorRole, Guid>> CreateRoleGroupsAsync(
        IReadOnlyList<Guid> partitions, CancellationToken cancellationToken)
    {
        Log.Info("setup", "Creating per-role user groups with permissions + partition access.");

        // Permission bundle each role needs. The entity write personas can all
        // fall back to creating the articles/containers/items they operate on
        // (the persona weights, not the permissions, make each role behave
        // differently), so they share the create/edit/delete item + article
        // baseline. The bulk handler additionally reads item events; the auditor
        // is read-only.
        var entityWriteBundle = new[]
        {
            FargoCodes.CreateArticle, FargoCodes.EditArticle,
            FargoCodes.CreateItem, FargoCodes.EditItem, FargoCodes.DeleteItem,
        };

        var bundles = new Dictionary<OperatorRole, int[]>
        {
            [OperatorRole.Receiver] = entityWriteBundle,
            [OperatorRole.PutAway] = entityWriteBundle,
            [OperatorRole.Picker] = entityWriteBundle,
            [OperatorRole.BulkHandler] =
                [FargoCodes.CreateArticle, FargoCodes.EditArticle, FargoCodes.CreateItem,
                 FargoCodes.EditItem, FargoCodes.ReadItemEvents],
            [OperatorRole.Auditor] =
                [FargoCodes.ReadAuditLogs, FargoCodes.ReadItemEvents],
        };

        var groupByRole = new Dictionary<OperatorRole, Guid>();
        foreach (var (role, bundle) in bundles)
        {
            var guid = await _admin.V1.UserGroups.PostAsync(
                new UserGroupCreateDto
                {
                    Nameid = _names.Nameid($"sim-{role}".ToLowerInvariant()),
                    Description = $"Simulation {role} operators.",
                    IsActive = true,
                    PermissionsToAdd = [.. bundle.Select(b => (int?)b)],
                    PartitionAccessesToAdd = [.. partitions.Select(p => (Guid?)p)],
                },
                cancellationToken: cancellationToken)
                ?? throw new InvalidOperationException($"Failed to create {role} group.");
            groupByRole[role] = guid;
        }

        return groupByRole;
    }

    private async Task<List<OperatorAccount>> CreateOperatorsAsync(
        IReadOnlyList<Guid> partitions,
        IReadOnlyDictionary<OperatorRole, Guid> groupByRole,
        CancellationToken cancellationToken)
    {
        var roles = BuildRoleRoster();
        Log.Info("setup", $"Creating {roles.Count} operator users.");

        // Operators act inside the zones (skip index 0, the site root) so the
        // partition-access grants exercise the scoping machinery.
        var zones = partitions.Skip(1).ToList();
        if (zones.Count == 0)
        {
            zones = [.. partitions];
        }

        var accounts = new List<OperatorAccount>(roles.Count);
        for (var i = 0; i < roles.Count; i++)
        {
            var role = roles[i];
            var nameid = _names.Nameid($"op-{role}".ToLowerInvariant());
            var partition = zones[i % zones.Count];

            var guid = await _admin.V1.Users.PostAsync(
                new UserCreateDto
                {
                    Nameid = nameid,
                    FirstName = _names.FirstName(i),
                    LastName = "Operator",
                    Description = $"Simulation {role} operator #{i}.",
                    IsActive = true,
                    Authentication = new UserAuthenticationCreateDto
                    {
                        Password = _options.NewUserPassword,
                        DefaultPasswordExpirationPeriod = "365.00:00:00",
                    },
                    UserGroupsToAdd = [groupByRole[role]],
                },
                cancellationToken: cancellationToken)
                ?? throw new InvalidOperationException($"Failed to create operator '{nameid}'.");

            _ = guid;
            await ClearPasswordChangeRequirementAsync(nameid, cancellationToken);

            accounts.Add(new OperatorAccount(i, nameid, role, partition));
        }

        // Prove each operator can authenticate and read its partition.
        foreach (var account in accounts)
        {
            await _check.AllowedAsync(account.Nameid, "operator can log in and read its partition", async () =>
            {
                var client = await _clientBuilder.CreateAuthenticatedClientAsync(
                    account.Nameid, _options.NewUserPassword, cancellationToken);
                await client.V1.Partitions[account.PartitionGuid].GetAsync(cancellationToken: cancellationToken);
            });
        }

        return accounts;
    }

    /// <summary>
    /// Builds the ordered list of roles for the configured operator count,
    /// honouring which flows are enabled and round-robining across the enabled
    /// personas so the mix stays balanced.
    /// </summary>
    private List<OperatorRole> BuildRoleRoster()
    {
        var enabled = new List<OperatorRole>();
        if (_options.EnableEntityFlow)
        {
            enabled.Add(OperatorRole.Receiver);
            enabled.Add(OperatorRole.PutAway);
            enabled.Add(OperatorRole.Picker);
        }
        if (_options.EnableEventFlow)
        {
            enabled.Add(OperatorRole.BulkHandler);
        }
        if (_options.EnableAuditorFlow)
        {
            enabled.Add(OperatorRole.Auditor);
        }
        if (enabled.Count == 0)
        {
            // Nothing enabled explicitly: fall back to the entity receiver so the
            // simulation still does something meaningful.
            enabled.Add(OperatorRole.Receiver);
        }

        var count = Math.Max(1, _options.Operators);
        var roster = new List<OperatorRole>(count);
        for (var i = 0; i < count; i++)
        {
            roster.Add(enabled[i % enabled.Count]);
        }
        return roster;
    }

    private async Task ClearPasswordChangeRequirementAsync(string nameid, CancellationToken cancellationToken)
    {
        await _admin.V1.Identity.Password.PutAsync(
            new IdentityPasswordUpdateDto
            {
                Nameid = nameid,
                CurrentPassword = _options.NewUserPassword,
                NewPassword = _options.NewUserPassword,
            },
            cancellationToken: cancellationToken);
    }
}
