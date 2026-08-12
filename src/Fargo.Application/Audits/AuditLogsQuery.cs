using Fargo.Application.Common;
using Fargo.Application.Shared.Articles;
using Fargo.Core.Shared.Actors;
using Fargo.Core.Shared.Entities;

namespace Fargo.Application.Audits;

public sealed record AuditLogsQuery(
    Pagination WithPagination,
    IReadOnlyCollection<Guid>? EntityGuids = null,
    IReadOnlyCollection<EntityType>? EntityType = null,
    IReadOnlyCollection<Guid>? ActorGuids = null,
    IReadOnlyCollection<ActorType>? ActorTypes = null
) : IQuery<IReadOnlyCollection<ArticleDto>>;
