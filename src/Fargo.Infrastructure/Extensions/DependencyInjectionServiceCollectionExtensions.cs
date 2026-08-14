using Fargo.Application.Articles;
using Fargo.Application.Audits;
using Fargo.Application.Common;
using Fargo.Application.Identity;
using Fargo.Application.Items;
using Fargo.Application.Partitions;
using Fargo.Application.UserGroups;
using Fargo.Application.Users;
using Fargo.Core.Articles;
using Fargo.Core.Audits;
using Fargo.Core.Identity;
using Fargo.Core.Items;
using Fargo.Core.Partitions;
using Fargo.Core.Security;
using Fargo.Core.UserGroups;
using Fargo.Core.Users;
using Fargo.Infrastructure.Persistence;
using Fargo.Infrastructure.Repositories;
using Fargo.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

            return services;
        }

        private IServiceCollection AddFargoJwtOptions(IConfiguration configuration)
        {
            services
            .AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations();

            return services;
        }

        public IServiceCollection AddFargoUnitOfWork() =>
            services.AddScoped<IUnitOfWork, UnitOfWork>();

        public IServiceCollection AddFargoDbContext(IConfiguration configuration) =>
            services.AddDbContext<FargoDbContext>((sp, opt) =>
            {
                opt.UseNpgsql(
                    configuration.GetConnectionString("fargo"),
                    npgsqlOptions =>
                    {
                        npgsqlOptions.MigrationsHistoryTable("__ef_migrations_history");
                    }
                ).UseSnakeCaseNamingConvention();
            });

        public void AddFargoRepositories() => services
            .AddScoped<IArticleRepository, ArticleRepository>()
            .AddScoped<IArticleQueryRepository, ArticleRepository>()
            .AddScoped<IItemRepository, ItemRepository>()
            .AddScoped<IItemQueryRepository, ItemRepository>()
            .AddScoped<IUserRepository, UserRepository>()
            .AddScoped<IUserQueryRepository, UserRepository>()
            .AddScoped<IUserGroupRepository, UserGroupRepository>()
            .AddScoped<IUserGroupQueryRepository, UserGroupRepository>()
            .AddScoped<IRefreshTokenRepository, RefreshTokenRepository>()
            .AddScoped<IPartitionRepository, PartitionRepository>()
            .AddScoped<IPartitionQueryRepository, PartitionRepository>()
            .AddScoped<IAuditLogRepository, AuditLogRepository>()
            .AddScoped<IAuditLogQueryRepository, AuditLogRepository>();

        public void AddFargoSecurity() => services
            .AddScoped<IPasswordHasher, IdentityPasswordHasher>()
            .AddScoped<ITokenGenerator, JwtTokenGenerator>()
            .AddScoped<ITokenHasher, Sha256TokenHasher>()
            .AddScoped<IRefreshTokenGenerator, CryptoRefreshTokenGenerator>();
    }
}
