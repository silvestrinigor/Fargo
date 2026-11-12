using Fargo.Application.Persistence;
using Fargo.Application.Persistence.Repositories;
using Fargo.Core.Audits;
using Fargo.Core.Entities.Actors.ServiceAccounts;
using Fargo.Core.Entities.Actors.Users;
using Fargo.Core.Entities.Articles;
using Fargo.Core.Entities.Items;
using Fargo.Core.Entities.Partitions;
using Fargo.Core.Entities.SupplyOrders;
using Fargo.Core.Entities.UserGroups;
using Fargo.Core.Events;
using Fargo.Core.Identity.Authentication;
using Fargo.Core.Identity.Tokens;
using Fargo.Infrastructure.Outbox;
using Fargo.Infrastructure.Persistence;
using Fargo.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Fargo.Infrastructure.DependencyInjection;

public static class PersistenceServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        internal IServiceCollection AddFargoPersistence(IConfiguration configuration) => services
        .AddFargoDbContext(configuration)
        .AddFargoUnitOfWork()
        .AddFargoRepositories()
        .AddFargoOutboxRepository();

        private IServiceCollection AddFargoDbContext(IConfiguration configuration)
        {
            // Stateless, so one instance serves every context.
            services.TryAddSingleton(TimeProvider.System);
            services.TryAddSingleton<OutboxSaveChangesInterceptor>();
            services.TryAddSingleton<AuthorizationStampSaveChangesInterceptor>();

            return services.AddDbContext<FargoDbContext>((sp, opt) =>
            {
                opt.UseNpgsql(
                    configuration.GetConnectionString("fargo-database"),
                    npgsqlOptions =>
                    {
                        npgsqlOptions.MigrationsHistoryTable("__ef_migrations_history");
                    }
                ).UseSnakeCaseNamingConvention();

                opt.AddInterceptors(
                    sp.GetRequiredService<OutboxSaveChangesInterceptor>(),
                    sp.GetRequiredService<AuthorizationStampSaveChangesInterceptor>());
            });
        }

        private IServiceCollection AddFargoUnitOfWork() =>
            services.AddScoped<IUnitOfWork, UnitOfWork>();

        private IServiceCollection AddFargoRepositories() => services
        .AddScoped<IArticleRepository, ArticleRepository>()
        .AddScoped<IArticleQueryRepository, ArticleRepository>()
        .AddScoped<IItemRepository, ItemRepository>()
        .AddScoped<IItemQueryRepository, ItemRepository>()
        .AddScoped<IItemEventRepository, ItemEventRepository>()
        .AddScoped<IItemEventQueryRepository, ItemEventRepository>()
        .AddScoped<IUserRepository, UserRepository>()
        .AddScoped<IUserQueryRepository, UserRepository>()
        .AddScoped<IUserGroupRepository, UserGroupRepository>()
        .AddScoped<IUserGroupQueryRepository, UserGroupRepository>()
        .AddScoped<IRefreshTokenRepository, RefreshTokenRepository>()
        .AddScoped<IPartitionRepository, PartitionRepository>()
        .AddScoped<IPartitionQueryRepository, PartitionRepository>()
        .AddScoped<IAuditLogRepository, AuditLogRepository>()
        .AddScoped<IAuditLogQueryRepository, AuditLogRepository>()
        .AddScoped<IAuthenticationAttemptRepository, AuthenticationAttemptRepository>()
        .AddScoped<IServiceAccountRepository, ServiceAccountRepository>()
        .AddScoped<IServiceAccountQueryRepository, ServiceAccountRepository>()
        .AddScoped<ISupplyOrderRepository, SupplyOrderRepository>()
        .AddScoped<ISupplyOrderQueryRepository, SupplyOrderRepository>()
        .AddScoped<IIdempotencyRecordRepository, IdempotencyRecordRepository>();

        private IServiceCollection AddFargoOutboxRepository() =>
            services.AddScoped<IOutboxRepository, OutboxRepository>();
    }
}
