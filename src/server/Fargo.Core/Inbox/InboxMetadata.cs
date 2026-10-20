namespace Fargo.Core.Inbox;

/// <summary>
/// Represents a collection of named values that make up an inbox message payload.
/// </summary>
public sealed class InboxMetadata
{
    /// <summary>
    /// Gets the values contained in the metadata, keyed by property name.
    /// </summary>
    public IReadOnlyDictionary<string, InboxValue> Values => values;

    private readonly Dictionary<string, InboxValue> values = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="InboxMetadata"/> class.
    /// </summary>
    public InboxMetadata() { }
}
