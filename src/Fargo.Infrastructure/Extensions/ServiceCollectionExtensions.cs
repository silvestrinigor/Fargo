using Fargo.Application;
using Fargo.Application.Articles;
using Fargo.Application.Identity;
using Fargo.Application.Items;
using Fargo.Application.Partitions;
using Fargo.Application.System;
using Fargo.Application.UserGroups;
using Fargo.Application.Users;
using Fargo.Core;
using Fargo.Core.Articles;
using Fargo.Core.Identity;
using Fargo.Core.Items;
using Fargo.Core.Partitions;
using Fargo.Core.UserGroups;
using Fargo.Core.Users;
using Fargo.Infrastructure.Persistence;
using Fargo.Infrastructure.Repositories;
using Fargo.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Fargo.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddFargoInfrastructure(IConfiguration configuration)
        {
            AddFargoJwt(services, configuration);
            AddFargoConnectionStrings(services, configuration);
            AddDbContexts(services);
            AddRepositories(services);
            AddSecurity(services);
            AddPersistence(services);

            services.AddScoped<ICurrentUser, CurrentUser>();
            services.AddScoped<IAuthorizationContextFactory, AuthorizationContextFactory>();
            services.AddScoped<ICurrentAuthorizationContext, CurrentAuthorizationContext>();

            return services;
        }

        public IServiceCollection AddFargoMigrationInfrastructure(IConfiguration configuration)
        {
            AddFargoConnectionStrings(services, configuration);
            AddDbContexts(services);

            return services;
        }

        public IServiceCollection AddFargoSeedInfrastructure(IConfiguration configuration)
        {
            AddFargoConnectionStrings(services, configuration);
            AddDbContexts(services);
            AddRepositories(services);
            AddSecurity(services);
            AddPersistence(services);

            services.AddScoped<ICommandHandler<InitializeSystemCommand>, InitializeSystemCommandHandler>();

            return services;
        }

        private IServiceCollection AddFargoJwt(
                IConfiguration configuration
                )
        {
            services
                .AddOptions<JwtOptions>()
                .Bind(configuration.GetSection(JwtOptions.SectionName))
                .ValidateDataAnnotations()
                .Validate(
                        o => o.Key.Length >= 32,
                        "Jwt:Key must be at least 32 characters long.")
                .ValidateOnStart();

            return services;
        }

        private IServiceCollection AddFargoConnectionStrings(
                IConfiguration configuration)
        {
            services
                .AddOptions<ConnectionStringOptions>()
                .Bind(configuration.GetSection(ConnectionStringOptions.SectionName))
                .Validate(o => !string.IsNullOrWhiteSpace(o.Fargo),
                        "ConnectionStrings:Fargo must be provided.")
                .ValidateOnStart();

            return services;
        }

        private void AddDbContexts()
        {
            services.AddDbContext<FargoDbContext>((sp, opt) => ConfigureSqlServer(sp, opt));
        }

        private void AddRepositories()
        {
            services.AddScoped<IEntityEventRepository, EntityEventRepository>();
            services.AddScoped<IEntityPartitionEventRepository, EntityPartitionEventRepository>();
            services.AddScoped<IArticleRepository, ArticleRepository>();
            services.AddScoped<IArticleQueryRepository, ArticleRepository>();
            services.AddScoped<IItemRepository, ItemRepository>();
            services.AddScoped<IItemQueryRepository, ItemRepository>();
            services.AddScoped<IItemMovementRepository, ItemMovementRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserQueryRepository, UserRepository>();
            services.AddScoped<IUserGroupRepository, UserGroupRepository>();
            services.AddScoped<IUserGroupQueryRepository, UserGroupRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IPartitionRepository, PartitionRepository>();
            services.AddScoped<IPartitionQueryRepository, PartitionRepository>();
        }

        private void AddSecurity()
        {
            services.AddScoped<IPasswordHasher, IdentityPasswordHasher>();
            services.AddScoped<ITokenGenerator, JwtTokenGenerator>();
            services.AddScoped<ITokenHasher, Sha256TokenHasher>();
            services.AddScoped<IRefreshTokenGenerator, CryptoRefreshTokenGenerator>();
        }

        private void AddPersistence()
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();
        }

        private static void ConfigureSqlServer(IServiceProvider sp, DbContextOptionsBuilder opt)
        {
            var options = sp
                .GetRequiredService<IOptions<ConnectionStringOptions>>()
                .Value;

            opt.UseSqlServer(options.Fargo);
        }
    }
}
