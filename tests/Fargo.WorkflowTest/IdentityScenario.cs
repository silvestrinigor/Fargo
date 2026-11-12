using Fargo.ClientHttp;
using Fargo.ClientHttp.Models;

namespace Fargo.WorkflowTest;

/// <summary>
/// Exercises the identity and authorization surface of the Fargo system:
/// partitions, user groups, users, permission grants/revocations, and the two
/// independent gates the server enforces on every write — the required
/// <c>ActionType</c> permission and partition access.
///
/// The scenario runs as the admin (passed in as <paramref name="admin"/>) to set
/// everything up, then logs in AS the freshly created restricted users to prove
/// that permitted operations succeed and forbidden ones are rejected.
/// </summary>
internal sealed class IdentityScenario(
    FargoApiClient admin,
    FargoClientBuilder clientBuilder,
    WorkflowOptions options,
    string runTag,
    Check check)
{
    // Well-known seeded global partition (Fargo.Core FargoCoreWellKnowGuids).
    private static readonly Guid GlobalPartitionGuid = Guid.Parse("00000000-0000-0000-0000-000000000002");

    private readonly FargoApiClient _admin = admin;
    private readonly FargoClientBuilder _clientBuilder = clientBuilder;
    private readonly WorkflowOptions _options = options;
    private readonly string _tag = runTag;
    private readonly Check _check = check;

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        // 1. Partitions: a two-level hierarchy under the global partition.
        var (warehouseGuid, zoneAGuid, zoneBGuid) = await CreatePartitionsAsync(cancellationToken);

        // 2. User groups with specific permissions + partition access.
        var inventoryGroupGuid = await CreateInventoryGroupAsync(zoneAGuid, cancellationToken);

        // 3. Users: one in the group, one restricted, one read-only.
        var operatorUser = await CreateOperatorUserAsync(inventoryGroupGuid, cancellationToken);
        var limitedUser = await CreateLimitedUserAsync(zoneBGuid, cancellationToken);

        // 4. Permission mutation: add then remove a permission on a group/user.
        await ExercisePermissionMutationAsync(inventoryGroupGuid, limitedUser.Guid, cancellationToken);

        // 5. Permission enforcement: log in as each user and assert allowed vs denied.
        await VerifyOperatorPermissionsAsync(operatorUser, zoneAGuid, cancellationToken);
        await VerifyLimitedUserDeniedAsync(limitedUser, cancellationToken);

        // 6. Partition access scoping: an entity in a zone is visible only to
        //    actors with access to that zone.
        await VerifyPartitionAccessScopingAsync(operatorUser, zoneAGuid, zoneBGuid, cancellationToken);

        // 7. Clean up the hierarchy to also exercise the delete endpoints.
        await ExerciseDeletesAsync(operatorUser.Guid, limitedUser.Guid, inventoryGroupGuid,
            zoneAGuid, zoneBGuid, warehouseGuid, cancellationToken);
    }

    private async Task<(Guid warehouse, Guid zoneA, Guid zoneB)> CreatePartitionsAsync(
        CancellationToken cancellationToken)
    {
        Log.Step("Creating a partition hierarchy (Warehouse > Zone-A, Zone-B)");

        var warehouse = await _admin.V1.Partitions.PostAsync(
            new PartitionCreateDto
            {
                Name = N("Warehouse"),
                Description = "Top-level warehouse partition.",
                // null parent -> defaults to the global partition server-side.
            },
            cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Failed to create warehouse partition.");

        var zoneA = await _admin.V1.Partitions.PostAsync(
            new PartitionCreateDto { Name = N("Zone-A"), ParentPartitionGuid = warehouse },
            cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Failed to create Zone-A partition.");

        var zoneB = await _admin.V1.Partitions.PostAsync(
            new PartitionCreateDto { Name = N("Zone-B"), ParentPartitionGuid = warehouse },
            cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Failed to create Zone-B partition.");

        Log.Success($"Warehouse {Short(warehouse)}, Zone-A {Short(zoneA)}, Zone-B {Short(zoneB)}");

        // Verify the hierarchy was persisted as requested.
        var zoneADto = await _admin.V1.Partitions[zoneA].GetAsync(cancellationToken: cancellationToken);
        _check.Equal(warehouse, zoneADto?.ParentPartitionGuid, "Zone-A parent is Warehouse");

        // Editing a partition (PATCH) requires EditPartition; admin has it.
        await _check.AllowedAsync("admin edits Zone-A description", () =>
            _admin.V1.Partitions[zoneA].PatchAsync(
                new PartitionUpdateDto { Description = "Primary picking zone." },
                cancellationToken: cancellationToken));

        return (warehouse, zoneA, zoneB);
    }

    private async Task<Guid> CreateInventoryGroupAsync(Guid zoneAGuid, CancellationToken cancellationToken)
    {
        Log.Step("Creating an 'inventory-operators' user group with item permissions + Zone-A access");

        var groupGuid = await _admin.V1.UserGroups.PostAsync(
            new UserGroupCreateDto
            {
                Nameid = N("inventory-operators"),
                Description = "Operators who manage items within Zone-A.",
                IsActive = true,
                PermissionsToAdd = [ActionTypes.CreateArticle, ActionTypes.CreateItem, ActionTypes.EditItem],
                PartitionAccessesToAdd = [zoneAGuid],
            },
            cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Failed to create inventory-operators group.");

        Log.Success($"Group inventory-operators {Short(groupGuid)}");

        var dto = await _admin.V1.UserGroups[groupGuid].GetAsync(cancellationToken: cancellationToken);
        _check.That(dto is not null, "inventory-operators group is retrievable");
        _check.That(
            dto?.Permissions is not null && dto.Permissions.Contains(ActionTypes.EditItem),
            "group has EditItem permission");

        return groupGuid;
    }

    private async Task<CreatedUser> CreateOperatorUserAsync(Guid groupGuid, CancellationToken cancellationToken)
    {
        Log.Step("Creating an operator user (member of inventory-operators)");

        var nameid = N("operator");

        var guid = await _admin.V1.Users.PostAsync(
            new UserCreateDto
            {
                Nameid = nameid,
                FirstName = "Olivia",
                LastName = "Operator",
                Description = "Inventory operator created by the workflow test.",
                IsActive = true,
                Authentication = new UserAuthenticationCreateDto
                {
                    Password = _options.NewUserPassword,
                    // .NET TimeSpan "c" format (days.hh:mm:ss): ~1 year, so the
                    // account stays usable after the mandatory first password
                    // change resets the expiration window.
                    DefaultPasswordExpirationPeriod = "365.00:00:00",
                },
                UserGroupsToAdd = [groupGuid],
            },
            cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Failed to create operator user.");

        Log.Success($"Operator user {nameid} {Short(guid)} (inherits permissions + Zone-A access from group)");

        await ClearPasswordChangeRequirementAsync(nameid, cancellationToken);

        return new CreatedUser(guid, nameid);
    }

    private async Task<CreatedUser> CreateLimitedUserAsync(Guid zoneBGuid, CancellationToken cancellationToken)
    {
        Log.Step("Creating a limited user (Zone-B access, no item/article permissions)");

        var nameid = N("limited");

        var guid = await _admin.V1.Users.PostAsync(
            new UserCreateDto
            {
                Nameid = nameid,
                Description = "Limited user with no write permissions.",
                IsActive = true,
                Authentication = new UserAuthenticationCreateDto
                {
                    Password = _options.NewUserPassword,
                    DefaultPasswordExpirationPeriod = "365.00:00:00",
                },
                // Access to Zone-B only; deliberately NO permissions granted.
                PartitionAccessesToAdd = [zoneBGuid],
            },
            cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Failed to create limited user.");

        Log.Success($"Limited user {nameid} {Short(guid)}");

        await ClearPasswordChangeRequirementAsync(nameid, cancellationToken);

        return new CreatedUser(guid, nameid);
    }

    private async Task ExercisePermissionMutationAsync(
        Guid groupGuid, Guid limitedUserGuid, CancellationToken cancellationToken)
    {
        Log.Step("Adding and removing permissions (group + user) via update endpoints");

        // Add DeleteItem to the group, confirm, then remove it again.
        await _check.AllowedAsync("admin grants DeleteItem to group", () =>
            _admin.V1.UserGroups[groupGuid].PutAsync(
                new UserGroupUpdateDto { PermissionsToAdd = [ActionTypes.DeleteItem] },
                cancellationToken: cancellationToken));

        var afterAdd = await _admin.V1.UserGroups[groupGuid].GetAsync(cancellationToken: cancellationToken);
        _check.That(
            afterAdd?.Permissions?.Contains(ActionTypes.DeleteItem) == true,
            "group has DeleteItem after grant");

        await _check.AllowedAsync("admin revokes DeleteItem from group", () =>
            _admin.V1.UserGroups[groupGuid].PutAsync(
                new UserGroupUpdateDto { PermissionsToRemove = [ActionTypes.DeleteItem] },
                cancellationToken: cancellationToken));

        var afterRemove = await _admin.V1.UserGroups[groupGuid].GetAsync(cancellationToken: cancellationToken);
        _check.That(
            afterRemove?.Permissions?.Contains(ActionTypes.DeleteItem) != true,
            "group no longer has DeleteItem after revoke");

        // Grant a direct permission to the limited user, then revoke it.
        await _check.AllowedAsync("admin grants direct CreateArticle to limited user", () =>
            _admin.V1.Users[limitedUserGuid].PutAsync(
                new UserUpdateDto { PermissionsToAdd = [ActionTypes.CreateArticle] },
                cancellationToken: cancellationToken));

        await _check.AllowedAsync("admin revokes direct CreateArticle from limited user", () =>
            _admin.V1.Users[limitedUserGuid].PutAsync(
                new UserUpdateDto { PermissionsToRemove = [ActionTypes.CreateArticle] },
                cancellationToken: cancellationToken));
    }

    private async Task VerifyOperatorPermissionsAsync(
        CreatedUser operatorUser, Guid zoneAGuid, CancellationToken cancellationToken)
    {
        Log.Step($"Logging in AS '{operatorUser.Nameid}' to verify granted permissions");

        var client = await _clientBuilder.CreateAuthenticatedClientAsync(
            operatorUser.Nameid, _options.NewUserPassword, cancellationToken);

        // The operator has CreateArticle + CreateItem + EditItem and Zone-A access.
        // Entities must live in Zone-A (not just the global partition) or the
        // operator's own access check rejects them — so stamp Zone-A on both.
        Guid articleGuid = Guid.Empty;
        await _check.AllowedAsync("operator creates an article in Zone-A", async () =>
        {
            articleGuid = await client.V1.Articles.PostAsync(
                new ArticleCreateDto
                {
                    Name = N("op-widget"),
                    ArticleType = 1,
                    PartitionsToAdd = [zoneAGuid],
                },
                cancellationToken: cancellationToken) ?? Guid.Empty;
        });

        Guid itemGuid = Guid.Empty;
        await _check.AllowedAsync("operator creates an item in Zone-A", async () =>
        {
            itemGuid = await client.V1.Items.PostAsync(
                new ItemCreateDto { ArticleGuid = articleGuid, PartitionsToAdd = [zoneAGuid] },
                cancellationToken: cancellationToken) ?? Guid.Empty;
        });

        // EditItem: the operator can read back and the item is in Zone-A.
        await _check.AllowedAsync("operator reads back its item", () =>
            client.V1.Items[itemGuid].GetAsync(cancellationToken: cancellationToken));

        // The operator lacks CreateUser / CreateUserGroup / CreatePartition.
        await _check.DeniedAsync("operator creates a user (lacks CreateUser)", () =>
            client.V1.Users.PostAsync(
                new UserCreateDto { Nameid = N("op-sneaky"), IsActive = true },
                cancellationToken: cancellationToken),
            401, 403);

        await _check.DeniedAsync("operator creates a user group (lacks CreateUserGroup)", () =>
            client.V1.UserGroups.PostAsync(
                new UserGroupCreateDto { Nameid = N("op-group") },
                cancellationToken: cancellationToken),
            401, 403);

        await _check.DeniedAsync("operator creates a partition (lacks CreatePartition)", () =>
            client.V1.Partitions.PostAsync(
                new PartitionCreateDto { Name = N("op-part") },
                cancellationToken: cancellationToken),
            401, 403);
    }

    private async Task VerifyLimitedUserDeniedAsync(CreatedUser limitedUser, CancellationToken cancellationToken)
    {
        Log.Step($"Logging in AS '{limitedUser.Nameid}' to verify it is denied write operations");

        var client = await _clientBuilder.CreateAuthenticatedClientAsync(
            limitedUser.Nameid, _options.NewUserPassword, cancellationToken);

        // No permissions at all: every create must be rejected.
        await _check.DeniedAsync("limited user creates an article (no permission)", () =>
            client.V1.Articles.PostAsync(
                new ArticleCreateDto { Name = N("lim-widget"), ArticleType = 1 },
                cancellationToken: cancellationToken),
            401, 403);

        await _check.DeniedAsync("limited user creates an item (no permission)", () =>
            client.V1.Items.PostAsync(
                new ItemCreateDto { ArticleGuid = Guid.NewGuid() },
                cancellationToken: cancellationToken),
            401, 403);
    }

    private async Task VerifyPartitionAccessScopingAsync(
        CreatedUser operatorUser, Guid zoneAGuid, Guid zoneBGuid, CancellationToken cancellationToken)
    {
        Log.Step("Verifying partition-access scoping (operator has Zone-A, not Zone-B)");

        var opClient = await _clientBuilder.CreateAuthenticatedClientAsync(
            operatorUser.Nameid, _options.NewUserPassword, cancellationToken);

        // The operator can read Zone-A (has access) but not Zone-B. A read the
        // actor cannot see returns 404 (partition-scoped filter), while lacking
        // the generic read is surfaced as 401/403 — accept any of them as "denied".
        await _check.AllowedAsync("operator reads Zone-A (has access)", () =>
            opClient.V1.Partitions[zoneAGuid].GetAsync(cancellationToken: cancellationToken));

        await _check.DeniedAsync("operator reads Zone-B (no access)", () =>
            opClient.V1.Partitions[zoneBGuid].GetAsync(cancellationToken: cancellationToken),
            401, 403, 404);
    }

    private async Task ExerciseDeletesAsync(
        Guid operatorGuid, Guid limitedGuid, Guid groupGuid,
        Guid zoneAGuid, Guid zoneBGuid, Guid warehouseGuid,
        CancellationToken cancellationToken)
    {
        Log.Step("Cleaning up: deleting users, group, and partitions (admin)");

        await _check.AllowedAsync("admin deletes operator user", () =>
            _admin.V1.Users[operatorGuid].DeleteAsync(cancellationToken: cancellationToken));

        await _check.AllowedAsync("admin deletes limited user", () =>
            _admin.V1.Users[limitedGuid].DeleteAsync(cancellationToken: cancellationToken));

        await _check.AllowedAsync("admin deletes inventory-operators group", () =>
            _admin.V1.UserGroups[groupGuid].DeleteAsync(cancellationToken: cancellationToken));

        // Delete leaf partitions before their parent.
        await _check.AllowedAsync("admin deletes Zone-A partition", () =>
            _admin.V1.Partitions[zoneAGuid].DeleteAsync(cancellationToken: cancellationToken));

        await _check.AllowedAsync("admin deletes Zone-B partition", () =>
            _admin.V1.Partitions[zoneBGuid].DeleteAsync(cancellationToken: cancellationToken));

        await _check.AllowedAsync("admin deletes Warehouse partition", () =>
            _admin.V1.Partitions[warehouseGuid].DeleteAsync(cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Clears the mandatory first-login password-change flag for a newly created
    /// user by performing the change-password flow (same old/new password is
    /// allowed and resets the expiration), so the user can then authenticate.
    /// </summary>
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

        Log.Info($"Cleared password-change requirement for '{nameid}'.");
    }

    /// <summary>Appends the run tag to a base name so repeated runs stay unique.</summary>
    private string N(string baseName) => $"{baseName}-{_tag}";

    private static string Short(Guid guid) => guid.ToString("N")[..8];

    private sealed record CreatedUser(Guid Guid, string Nameid);
}
