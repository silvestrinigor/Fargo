namespace Fargo.Infrastructure.Persistence;

/// <summary>
/// Configuration options used to resolve database connection strings.
/// </summary>
/// <remarks>
/// These settings are typically bound from application configuration,
/// such as <c>appsettings.json</c>, environment variables, or user secrets.
/// </remarks>
public sealed class ConnectionStringOptions
{
    /// <summary>
    /// The configuration section name used for binding these options.
    /// </summary>
    public const string SectionName = "ConnectionStrings";

    /// <summary>
    /// Gets the connection string used by the Fargo database.
    /// </summary>
    public required string Fargo { get; init; }
}
