using Fargo.Application.Common;

namespace Fargo.Application.Audits;

/// <summary>
/// Represents a query for retrieving a single audit log by its unique identifier.
/// </summary>
/// <param name="AuditLogGuid">The unique identifier of the audit log to retrieve.</param>
public sealed record AuditLogSingleQuery(Guid AuditLogGuid) : IQuery<AuditLogDto?>;
