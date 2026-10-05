namespace Fargo.Core.Inbox;

public sealed class InboxMetadata
{
    public IReadOnlyDictionary<string, InboxValue> Values => values;

    private readonly Dictionary<string, InboxValue> values = [];

    public InboxMetadata() { }
}
