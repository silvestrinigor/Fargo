using Fargo.Application;
using Fargo.Application.Articles;
using Fargo.Application.Identity;
using Fargo.Application.Items;
using Fargo.Application.Partitions;
using Fargo.Application.System;
using Fargo.Application.UserGroups;
using Fargo.Application.Users;
using Fargo.Core.Articles;
using Fargo.Core.Events;
using Fargo.Core.Identity;
using Fargo.Core.Items;
using Fargo.Core.Partitions;
using Fargo.Core.UserGroups;
using Fargo.Core.Users;
using Fargo.Infrastructure.Articles;
using Fargo.Infrastructure.Persistence;
using Fargo.Infrastructure.Repositories;
using Fargo.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

        public IServiceCollection AddFargoAuthentication(IConfiguration configuration)
        {
            AddFargoJwt(services, configuration);

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer();

            services.ConfigureOptions<JwtBearerOptionsSetup>();

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
            var optionsBuilder = services
                .AddOptions<JwtOptions>()
                .Bind(configuration.GetSection(JwtOptions.SectionName))
                .ValidateDataAnnotations()
                .Validate(
                    options => !string.IsNullOrWhiteSpace(options.Key),
                    "Jwt:Key is required.")
                .Validate(
                    options => !string.IsNullOrWhiteSpace(options.Key) && options.Key.Length >= 32,
                    "Jwt:Key must be at least 32 characters long.")
                .Validate(
                    options => !string.IsNullOrWhiteSpace(options.Issuer),
                    "Jwt:Issuer is required.")
                .Validate(
                    options => !string.IsNullOrWhiteSpace(options.Audience),
                    "Jwt:Audience is required.");

            if (!IsOpenApiDocumentGeneration())
            {
                optionsBuilder.ValidateOnStart();
            }

            return services;
        }

        private IServiceCollection AddFargoConnectionStrings(
                IConfiguration configuration)
        {
            var optionsBuilder = services
                .AddOptions<ConnectionStringOptions>()
                .Bind(configuration.GetSection(ConnectionStringOptions.SectionName))
                .Validate(o => !string.IsNullOrWhiteSpace(o.Fargo),
                        "ConnectionStrings:Fargo must be provided.");

            if (!IsOpenApiDocumentGeneration())
            {
                optionsBuilder.ValidateOnStart();
            }

            return services;
        }

        private void AddDbContexts()
        {
            services.AddDbContext<FargoDbContext>((sp, opt) => ConfigureSqlServer(sp, opt));
        }

        private void AddRepositories()
        {
            services.AddScoped<IEventRepository, EntityEventRepository>();
            services.AddScoped<IPartitionEventRepository, EntityPartitionEventRepository>();
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

    private static bool IsOpenApiDocumentGeneration()
    {
        // The build-time OpenAPI tool starts the app without runtime secrets or connection strings.
        return AppDomain.CurrentDomain.GetAssemblies()
            .Any(assembly => string.Equals(
                assembly.GetName().Name,
                "GetDocument.Insider",
                StringComparison.OrdinalIgnoreCase));
    }
}
