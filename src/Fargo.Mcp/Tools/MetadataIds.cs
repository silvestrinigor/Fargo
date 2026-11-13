using Fargo.Client.Models;

namespace Fargo.Mcp.Tools;

/// <summary>
/// Resolves the numeric id of an enumeration member by name from the server's metadata catalogs, so
/// the tools never hard-code the codes.
/// </summary>
internal static class MetadataIds
{
    public static async Task<int> ResolveAsync(
        Func<Action<Microsoft.Kiota.Abstractions.RequestConfiguration<Microsoft.Kiota.Abstractions.DefaultQueryParameters>>?, CancellationToken, Task<List<CatalogEntryDto>?>> fetch,
        string name,
        CancellationToken cancellationToken)
    {
        var entries = await fetch(null, cancellationToken);

        var entry = entries?.FirstOrDefault(e => string.Equals(e.Name, name, StringComparison.OrdinalIgnoreCase));

        return entry?.Id ?? throw new InvalidOperationException($"The server does not list '{name}' in the requested metadata catalog.");
    }
}
