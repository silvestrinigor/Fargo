using System.Globalization;

namespace Fargo.Mcp.Tools;

/// <summary>
/// Parses the compact string tokens the MCP tools accept for API value objects
/// (measures, kit components, allowed articles) and converts plain arrays to the
/// nullable lists the generated client expects. Mirrors the formats the
/// <c>fargo</c> CLI accepts. Malformed input throws <see cref="ArgumentException"/>,
/// which the MCP SDK reports to the model as a tool error.
/// </summary>
internal static class ToolInput
{
    /// <summary>
    /// Description of the numeric permission codes (the server's <c>ActionType</c>
    /// enum, serialized as integers), shared by the user and service-account tools.
    /// </summary>
    public const string PermissionCodes =
        "Numeric permission (ActionType) codes: 1 CreateArticle, 2 DeleteArticle, 3 EditArticle, 4 CreateItem, 5 DeleteItem, 6 EditItem, "
        + "7 CreateUser, 8 DeleteUser, 9 EditUser, 10 ChangeAnotherUserPassword, 11 CreateUserGroup, 12 DeleteUserGroup, 13 EditUserGroup, "
        + "14 ChangeUserGroupMembers, 15 CreatePartition, 16 DeletePartition, 17 EditPartition, 18 ReadAuditLogs, 19 ReadItemEvents, "
        + "20 CreateServiceAccount, 21 DeleteServiceAccount, 22 EditServiceAccount, 23 ReadServiceAccount, 24 CreateSupplyOrder, "
        + "25 EditSupplyOrder, 26 PlaceSupplyOrder, 27 ReceiveSupplyOrder, 28 CancelSupplyOrder, 29 CloseSupplyOrder, 30 ReadSupplyOrders.";

    /// <summary>
    /// Splits a measure token such as <c>25kg</c> or <c>10 cm</c> into its numeric
    /// value and unit abbreviation.
    /// </summary>
    public static (double Value, string Unit) ParseMeasure(string token, string parameterName)
    {
        var trimmed = (token ?? string.Empty).Trim();

        var index = 0;
        while (index < trimmed.Length)
        {
            var c = trimmed[index];
            var isNumeric = char.IsDigit(c) || c == '.' || c == '-' || c == '+' || c == ',';
            if (!isNumeric)
            {
                break;
            }

            index++;
        }

        var numberPart = trimmed[..index];
        var unitPart = trimmed[index..].Trim();

        if (index == 0 || unitPart.Length == 0 ||
            !double.TryParse(numberPart, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
        {
            throw new ArgumentException(
                $"Invalid value for '{parameterName}': '{token}'. Expected a number followed by a unit, e.g. '25kg' or '10cm'.");
        }

        return (value, unitPart);
    }

    /// <summary>
    /// Parses a kit-component token <c>&lt;article-guid&gt;</c> or
    /// <c>&lt;article-guid&gt;:&lt;quantity&gt;</c>. The quantity is optional.
    /// </summary>
    public static (Guid ArticleGuid, double? Quantity) ParseKitComponent(string token)
    {
        var (articleGuid, quantity) = SplitGuidAndNumber(token, "kit component", "quantity");

        return (articleGuid, quantity);
    }

    /// <summary>
    /// Parses a container allowed-article token <c>&lt;article-guid&gt;</c> or
    /// <c>&lt;article-guid&gt;:&lt;max-quantity&gt;</c>. The max quantity is
    /// optional and, when present, must be positive.
    /// </summary>
    public static (Guid ArticleGuid, double? MaxQuantity) ParseAllowedArticle(string token)
    {
        var (articleGuid, maxQuantity) = SplitGuidAndNumber(token, "allowed article", "maximum quantity");

        if (maxQuantity is <= 0)
        {
            throw new ArgumentException(
                $"The allowed article maximum quantity in '{token}' must be greater than zero.");
        }

        return (articleGuid, maxQuantity);
    }

    /// <summary>Converts an array of GUIDs to the nullable list the client uses, or <see langword="null"/> when empty.</summary>
    public static List<Guid?>? ToGuidList(Guid[]? values) =>
        values is { Length: > 0 } ? values.Select(v => (Guid?)v).ToList() : null;

    /// <summary>Converts an array of ints to the nullable list the client uses, or <see langword="null"/> when empty.</summary>
    public static List<int?>? ToIntList(int[]? values) =>
        values is { Length: > 0 } ? values.Select(v => (int?)v).ToList() : null;

    private static (Guid Guid, double? Number) SplitGuidAndNumber(string token, string what, string numberName)
    {
        var trimmed = (token ?? string.Empty).Trim();

        if (trimmed.Length == 0)
        {
            throw new ArgumentException($"A {what} cannot be empty.");
        }

        var separator = trimmed.LastIndexOf(':');

        string guidPart;
        double? number = null;

        if (separator < 0)
        {
            guidPart = trimmed;
        }
        else
        {
            guidPart = trimmed[..separator].Trim();
            var numberPart = trimmed[(separator + 1)..].Trim();

            if (numberPart.Length > 0)
            {
                if (!double.TryParse(numberPart, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
                {
                    throw new ArgumentException(
                        $"Invalid {what} {numberName} '{numberPart}' in '{trimmed}'. Expected a number.");
                }

                number = parsed;
            }
        }

        if (!Guid.TryParse(guidPart, out var guid))
        {
            throw new ArgumentException(
                $"Invalid {what} '{trimmed}'. Expected '<article-guid>' or '<article-guid>:<{numberName.Replace(' ', '-')}>'.");
        }

        return (guid, number);
    }
}
