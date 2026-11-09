using Fargo.Application.Common;
using Fargo.Application.Common.Actors;
using Fargo.Application.IntegrationEvents;
using Fargo.Application.Operations.Articles;
using Fargo.Application.Operations.Audits;
using Fargo.Application.Operations.Identity;
using Fargo.Application.Operations.Items;
using Fargo.Application.Operations.Partitions;
using Fargo.Application.Operations.SupplyOrders;
using Fargo.Application.Operations.UserGroups;
using Fargo.Application.Operations.Users;
using Fargo.Application.Persistence;
using Fargo.Application.Persistence.Repositories;
using Fargo.Application.Security;
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
using Fargo.Core.Identity.Credentials;
using Fargo.Core.Identity.Tokens;
using Fargo.Infrastructure.Client;
using Fargo.Infrastructure.Messaging;
using Fargo.Infrastructure.Outbox;
using Fargo.Infrastructure.Persistence;
using Fargo.Infrastructure.Persistence.Repositories;
using Fargo.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;


namespace Fargo.Infrastructure.Extensions;

public static class DependencyInjectionServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddFargoInfrastructure(IConfiguration configuration)
        {
            services.AddFargoDbContext(configuration);

            services.AddFargoUnitOfWork();

            services.AddHttpContextAccessor();

            services.AddFargoRepositories();

            services.ConfigureOptions<JwtBearerOptionsSetup>();

            services.AddFargoJwtOptions(configuration);

            services.AddFargoSecurity();

            services.AddScoped<ICurrentActor, CurrentUserActor>();

            services.AddScoped<IClientContext, HttpClientContext>();

            return services;
        }

        private IServiceCollection AddFargoJwtOptions(IConfiguration configuration)
        {
            // ValidateDataAnnotations enforces the Key/Issuer/Audience rules on
            // first resolution at runtime (during authentication setup at
            // startup), so a missing or weak secret fails fast. ValidateOnStart
            // is intentionally not used: it would force validation during
            // build-time OpenAPI generation and design-time tooling, which run
            // without the Development/secret configuration loaded.
            services
            .AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations();

            return services;
        }

        public IServiceCollection AddFargoUnitOfWork() =>
            services.AddScoped<IUnitOfWork, UnitOfWork>();

        public IServiceCollection AddFargoDbContext(IConfiguration configuration)
        {
            // Stateless, so one instance serves every context.
            services.TryAddSingleton(TimeProvider.System);
            services.TryAddSingleton<OutboxSaveChangesInterceptor>();

            return services.AddDbContext<FargoDbContext>((sp, opt) =>
            {
                opt.UseNpgsql(
                    configuration.GetConnectionString("fargo-database"),
                    npgsqlOptions =>
                    {
                        npgsqlOptions.MigrationsHistoryTable("__ef_migrations_history");
                    }
                ).UseSnakeCaseNamingConvention();

                opt.AddInterceptors(sp.GetRequiredService<OutboxSaveChangesInterceptor>());
            });
        }

        public void AddFargoRepositories() => services
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
        .AddScoped<ISupplyOrderQueryRepository, SupplyOrderRepository>();

        public void AddFargoSecurity() => services
        .AddScoped<IPasswordHasher, IdentityPasswordHasher>()
        .AddScoped<ITokenGenerator, JwtTokenGenerator>()
        .AddScoped<ITokenHasher, Sha256TokenHasher>()
        .AddScoped<IRefreshTokenGenerator, CryptoRefreshTokenGenerator>()
        .AddScoped<IClientSecretGenerator, ClientSecretGenerator>();

        /// <summary>
        /// Registers the background relay that publishes outbox messages through
        /// the registered <see cref="IEventPublisher"/>. Register it in one host
        /// only (Fargo.ServiceOutbox); other hosts still write outbox messages.
        /// </summary>
        public IServiceCollection AddFargoOutboxRelay(IConfiguration configuration)
        {
            services
            .AddOptions<OutboxOptions>()
            .Bind(configuration.GetSection(OutboxOptions.SectionName))
            .ValidateDataAnnotations();

            services.TryAddSingleton(TimeProvider.System);

            services.AddScoped<OutboxRelay>();

            services.AddHostedService<OutboxRelayService>();

            return services;
        }

        /// <summary>
        /// Registers <see cref="RabbitMqEventPublisher"/> as the
        /// <see cref="IEventPublisher"/>. Requires an <see cref="IConnection"/>
        /// registration, for example from the Aspire RabbitMQ client integration.
        /// </summary>
        public IServiceCollection AddFargoRabbitMqEventPublisher(IConfiguration configuration)
        {
            services
            .AddOptions<RabbitMqEventOptions>()
            .Bind(configuration.GetSection(RabbitMqEventOptions.SectionName))
            .ValidateDataAnnotations();

            // The connection is resolved lazily, on the first publish, so the
            // host starts even while the broker is down.
            services.AddSingleton<IEventPublisher>(sp => new RabbitMqEventPublisher(
                () => sp.GetRequiredService<IConnection>(),
                sp.GetRequiredService<IOptions<RabbitMqEventOptions>>()));

            return services;
        }
    }
}
