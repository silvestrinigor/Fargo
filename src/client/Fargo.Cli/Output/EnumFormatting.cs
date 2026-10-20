using System.Text;

namespace Fargo.Cli.Output;

/// <summary>
/// Classification of an article. Mirrors the server's ArticleType enum.
/// </summary>
public enum ArticleType
{
    Default = 1,
    Variation = 2,
    Pack = 3,
    Kit = 4,
    Container = 5,
}

/// <summary>
/// Type of actor that performed an action. Mirrors the server's ActorType enum.
/// </summary>
public enum ActorType
{
    User = 1,
}

/// <summary>
/// Type of entity affected by an action. Mirrors the server's EntityType enum.
/// </summary>
public enum EntityType
{
    Article = 0,
    Item = 1,
    User = 2,
    UserGroup = 3,
    Partition = 4,
    AuditLog = 5,
}

/// <summary>
/// Action performed in the system. Mirrors the server's ActionType enum.
/// </summary>
public enum ActionType
{
    CreateArticle = 1,
    DeleteArticle = 2,
    EditArticle = 3,
    CreateItem = 4,
    DeleteItem = 5,
    EditItem = 6,
    CreateUser = 7,
    DeleteUser = 8,
    EditUser = 9,
    ChangeAnotherUserPassword = 10,
    CreateUserGroup = 11,
    DeleteUserGroup = 12,
    EditUserGroup = 13,
    ChangeUserGroupMembers = 14,
    CreatePartition = 15,
    DeletePartition = 16,
    EditPartition = 17,
    ReadAuditLogs = 18,
}

/// <summary>
/// Formats enum values as human-friendly, kebab-cased names for display, and
/// renders raw numeric codes through their matching enum name.
/// </summary>
public static class EnumFormatting
{
    /// <summary>
    /// Formats a raw numeric enum code as the kebab-cased name of the enum value
    /// (e.g. 3 -> "user-group"). Unknown codes are shown as "code (N)".
    /// </summary>
    public static string? Format<TEnum>(int? code) where TEnum : struct, Enum
    {
        if (code is null)
        {
            return null;
        }

        if (Enum.IsDefined(typeof(TEnum), code.Value))
        {
            var name = Enum.GetName(typeof(TEnum), code.Value)!;
            return ToKebabCase(name);
        }

        return $"unknown ({code.Value})";
    }

    /// <summary>Formats an enum value as its kebab-cased name.</summary>
    public static string Format<TEnum>(TEnum value) where TEnum : struct, Enum
    {
        return ToKebabCase(value.ToString());
    }

    /// <summary>
    /// Converts a PascalCase identifier to kebab-case
    /// (e.g. "UserGroup" -> "user-group", "ReadAuditLogs" -> "read-audit-logs").
    /// </summary>
    public static string ToKebabCase(string name)
    {
        var builder = new StringBuilder(name.Length + 8);

        for (var i = 0; i < name.Length; i++)
        {
            var c = name[i];

            if (char.IsUpper(c))
            {
                if (i > 0)
                {
                    builder.Append('-');
                }

                builder.Append(char.ToLowerInvariant(c));
            }
            else
            {
                builder.Append(c);
            }
        }

        return builder.ToString();
    }
}
