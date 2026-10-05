using Fargo.Core.Actors;
using Fargo.Core.Audits;
using Fargo.Core.Entities;

namespace Fargo.Application.Audits;

/// <summary>
/// Represents a data transfer object for audit log information.
/// This DTO contains all the essential details about an audit event, including
/// the actor who performed the action, the entity affected, the type of action,
/// when it occurred, and any additional metadata.
/// </summary>
/// <param name="Guid">The unique identifier of the audit log entry</param>
/// <param name="ActorGuid">The unique identifier of the actor who performed the action</param>
/// <param name="ActorType">The type of actor who performed the action (e.g., User, Application)</param>
/// <param name="ActionType">The type of action that was performed (e.g., Create, Update, Delete)</param>
/// <param name="EntityGuid">The unique identifier of the entity that was affected by the action</param>
/// <param name="EntityType">The type of entity that was affected (e.g., Article, User)</param>
/// <param name="OccurredAt">The date and time when the audit event occurred</param>
/// <param name="Metadata">A dictionary containing additional metadata about the audit event</param>
/// <param name="Partitions">The collection of partition GUIDs that this audit log entry belongs to</param>
public sealed record AuditLogDto(
    Guid Guid,
    Guid ActorGuid,
    ActorType ActorType,
    ActionType ActionType,
    Guid EntityGuid,
    EntityType EntityType,
    DateTimeOffset OccurredAt,
    IReadOnlyDictionary<string, AuditValue> Metadata,
    IReadOnlyCollection<Guid> Partitions
);
