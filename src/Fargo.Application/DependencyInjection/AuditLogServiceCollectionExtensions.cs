using Fargo.Application.Common.Paging;
using Fargo.Application.DependencyInjection;
using Fargo.Application.Dtos.Audits;
using Fargo.Application.Queries.Audits;
using Microsoft.Extensions.DependencyInjection;

namespace Fargo.Application.DependencyInjection;

public static class AuditLogServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddFargoAuditLogApplication() => services
        .AddFargoQueryHandler<AuditLogsQuery, PagedResult<AuditLogDto>, AuditLogsQueryHandler>()
        .AddFargoQueryHandler<AuditLogSingleQuery, AuditLogDto?, AuditLogSingleQueryHandler>();
    }
}
