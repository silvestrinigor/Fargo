using Fargo.Core.Entities;
using Fargo.Core.Informations;

namespace Fargo.Core.Inbox;

public class ComInbox : IEntity
{
    public Guid Guid { get; private init; } = Guid.NewGuid();

    public ComInboxType ComInboxType { get; init; }

    public ComInboxStatus Status { get; set; }

    public Description ErrorMessage { get; set; } = Description.Empty;

    public required InboxMetadata Data { get; set; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? ProcessedAt { get; set; }
}
