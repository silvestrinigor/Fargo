using Fargo.Cli.Output;
using Fargo.ClientHttp;
using Fargo.ClientHttp.Models;
using System.CommandLine;

namespace Fargo.Cli.Commands.UserGroups;

/// <summary>
/// Renders user groups as a hierarchy, nesting child groups under their parent
/// and member users under each group. Built from existing APIs only
/// (GET /user-groups and GET /users); no new backend endpoint is required.
/// </summary>
/// <remarks>
/// Group nesting uses <see cref="UserGroupDto.ParentUserGroup"/>, the parent
/// user-group GUID. User membership comes from
/// <see cref="UserDto.UserGroups"/>. Each node is tagged with its type
/// (<c>[group]</c> or <c>[user]</c>). Users that belong to no group appear at
/// the root level, as siblings of the top-level groups.
/// </remarks>
public sealed class TreeUserGroupsCommand : Command
{
    public TreeUserGroupsCommand(FargoApiClient client)
        : base("tree", """
            Show user groups as a tree: child groups nested under their parent,
            and member users nested under each group.

            Each line is tagged with its type ([group] or [user]). Users that
            are not in any group appear at the root level, alongside the
            top-level groups.

            Examples:
              fargo user-group tree
            """)
    {
        SetAction(async (_, cancellationToken) =>
        {
            var groups = (await client.UserGroups.GetAsync(cancellationToken: cancellationToken))?.Items ?? [];
            var users = (await client.Users.GetAsync(cancellationToken: cancellationToken))?.Items ?? [];

            // Index users by each group they belong to (UserDto.UserGroups).
            var usersByGroup = new Dictionary<Guid, List<UserDto>>();
            var ungrouped = new List<UserDto>();

            foreach (var user in users)
            {
                var memberships = user.UserGroups?
                    .Where(g => g.HasValue)
                    .Select(g => g!.Value)
                    .ToList() ?? [];

                if (memberships.Count == 0)
                {
                    ungrouped.Add(user);
                    continue;
                }

                foreach (var groupGuid in memberships)
                {
                    if (!usersByGroup.TryGetValue(groupGuid, out var list))
                    {
                        usersByGroup[groupGuid] = list = [];
                    }

                    list.Add(user);
                }
            }

            // Index groups and their parent/child relationships. The parent link
            // lives in ParentUserGroup (the parent user-group GUID).
            var groupsByGuid = new Dictionary<Guid, UserGroupDto>();
            var childrenByParent = new Dictionary<Guid, List<UserGroupDto>>();
            var rootGroups = new List<UserGroupDto>();

            foreach (var group in groups)
            {
                if (group.Guid is { } guid)
                {
                    groupsByGuid[guid] = group;
                }
            }

            foreach (var group in groups)
            {
                var parentGuid = group.ParentUserGroup;

                // A group is a root when it has no parent, or its parent is not
                // among the groups we can see (orphaned parent -> treat as root).
                if (parentGuid is { } pg && groupsByGuid.ContainsKey(pg))
                {
                    if (!childrenByParent.TryGetValue(pg, out var siblings))
                    {
                        childrenByParent[pg] = siblings = [];
                    }

                    siblings.Add(group);
                }
                else
                {
                    rootGroups.Add(group);
                }
            }

            var roots = new List<TreeNode>();
            var visited = new HashSet<Guid>();

            foreach (var group in rootGroups.OrderBy(g => g.Nameid, StringComparer.OrdinalIgnoreCase))
            {
                roots.Add(BuildGroupNode(group, childrenByParent, usersByGroup, visited));
            }

            // Users that belong to no group are shown directly at the root
            // level, as siblings of the top-level groups.
            foreach (var user in ungrouped.OrderBy(u => u.Nameid, StringComparer.OrdinalIgnoreCase))
            {
                roots.Add(new TreeNode(UserLabel(user)));
            }

            if (roots.Count == 0)
            {
                Console.WriteLine("No user groups or users found.");
                return 0;
            }

            TreeWriter.WriteForest(roots);
            return 0;
        });
    }

    /// <summary>
    /// Builds a tree node for a group: its child groups first, then its member
    /// users. Guards against cycles via <paramref name="visited"/>.
    /// </summary>
    private static TreeNode BuildGroupNode(
        UserGroupDto group,
        IReadOnlyDictionary<Guid, List<UserGroupDto>> childrenByParent,
        IReadOnlyDictionary<Guid, List<UserDto>> usersByGroup,
        HashSet<Guid> visited)
    {
        var node = new TreeNode(GroupLabel(group));

        if (group.Guid is not { } guid || !visited.Add(guid))
        {
            // Missing GUID or already visited (cycle) -> do not recurse.
            return node;
        }

        if (childrenByParent.TryGetValue(guid, out var childGroups))
        {
            foreach (var child in childGroups.OrderBy(g => g.Nameid, StringComparer.OrdinalIgnoreCase))
            {
                node.Add(BuildGroupNode(child, childrenByParent, usersByGroup, visited));
            }
        }

        if (usersByGroup.TryGetValue(guid, out var members))
        {
            foreach (var user in members.OrderBy(u => u.Nameid, StringComparer.OrdinalIgnoreCase))
            {
                node.Add(UserLabel(user));
            }
        }

        return node;
    }

    private static string GroupLabel(UserGroupDto group)
    {
        var name = group.Nameid ?? group.Guid?.ToString() ?? "(unknown)";
        var label = $"[group] {name}";

        if (group.IsAdminUserGroup == true)
        {
            label += " [admin]";
        }

        if (group.IsActive == false)
        {
            label += " (inactive)";
        }

        return label;
    }

    private static string UserLabel(UserDto user)
    {
        var name = user.Nameid ?? user.Guid?.ToString() ?? "(unknown)";
        var label = $"[user]  {name}";

        if (user.IsAdmin == true)
        {
            label += " [admin]";
        }

        if (user.IsActive == false)
        {
            label += " (inactive)";
        }

        return label;
    }
}
