using Fargo.Application.Common.Paging;
using Fargo.Application.DependencyInjection;
using Fargo.Application.Operations.Audits.Dtos;
using Microsoft.Extensions.DependencyInjection;

namespace Fargo.Application.Operations.Audits;

/// <summary>
/// Registers the Audits use-case handlers.
/// </summary>
public static class AuditLogServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddFargoAuditLogApplication() => services
        .AddFargoQueryHandler<AuditLogsQuery, PagedResult<AuditLogDto>, AuditLogsQueryHandler>()
        .AddFargoQueryHandler<AuditLogSingleQuery, AuditLogDto?, AuditLogSingleQueryHandler>();
    }
}
