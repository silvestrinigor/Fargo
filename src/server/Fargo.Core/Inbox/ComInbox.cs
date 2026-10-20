using Fargo.Core.Entities;
using Fargo.Core.Informations;

namespace Fargo.Core.Inbox;

/// <summary>
/// Represents a message received into the communication inbox for processing.
/// </summary>
/// <remarks>
/// Inbox messages capture inbound payloads together with their processing
/// status, allowing them to be handled asynchronously and to record any error
/// that occurs during processing.
/// </remarks>
public class ComInbox : IEntity
{
    /// <summary>
    /// Gets the unique identifier of the inbox message.
    /// </summary>
    public Guid Guid { get; private init; } = Guid.NewGuid();

    /// <summary>
    /// Gets the type of the inbox message.
    /// </summary>
    public ComInboxType ComInboxType { get; init; }

    /// <summary>
    /// Gets or sets the current processing status of the inbox message.
    /// </summary>
    public ComInboxStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the error message describing a processing failure.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="Description.Empty"/> when no error has occurred.
    /// </remarks>
    public Description ErrorMessage { get; set; } = Description.Empty;

    /// <summary>
    /// Gets or sets the payload data of the inbox message.
    /// </summary>
    public required InboxMetadata Data { get; set; }

    /// <summary>
    /// Gets the date and time, in UTC, at which the message was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Gets or sets the date and time, in UTC, at which the message was processed.
    /// </summary>
    /// <remarks>
    /// A <see langword="null"/> value indicates that the message has not yet been
    /// processed.
    /// </remarks>
    public DateTimeOffset? ProcessedAt { get; set; }
}
