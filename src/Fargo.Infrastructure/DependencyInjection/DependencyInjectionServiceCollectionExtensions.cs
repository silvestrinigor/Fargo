using Fargo.Application;
using Fargo.Application.Articles;
using Fargo.Application.Identity;
using Fargo.Application.Items;
using Fargo.Application.Partitions;
using Fargo.Application.UserGroups;
using Fargo.Application.Users;
using Fargo.Core.Articles;
using Fargo.Core.Identity;
using Fargo.Core.Items;
using Fargo.Core.Partitions;
using Fargo.Core.UserGroups;
using Fargo.Core.Users;
using Fargo.Infrastructure.Articles;
using Fargo.Infrastructure.Persistence;
using Fargo.Infrastructure.Repositories;
using Fargo.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Fargo.Infrastructure.Extensions;

public static class DependencyInjectionServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddFargoInfrastructure(
            IConfiguration configuration, Action<InfrastructureOptions>? configure = null)
        {
            var options = new InfrastructureOptions();

            configure?.Invoke(options);

            services.AddFargoConnectionStringOptions(configuration);

            services.AddFargoDbContext();

            services.AddFargoUnitOfWork();

            services.ConfigureOptions<JwtBearerOptionsSetup>();

            services.AddFargoRepositories();

            services.AddFargoJwtOptions(configuration);

            services.AddFargoSecurity();

            options.ConfigureCurrentUser?.Invoke(services);

            return services;
        }

        private IServiceCollection AddFargoJwtOptions(
                IConfiguration configuration)
        {
            services
                .AddOptions<JwtOptions>()
                .Bind(configuration.GetSection(JwtOptions.SectionName))
                .ValidateDataAnnotations();

            return services;
        }

        public IServiceCollection AddFargoUnitOfWork() => services
            .AddScoped<IUnitOfWork, UnitOfWork>();

        public IServiceCollection AddFargoDbContext() => services
            .AddDbContext<FargoDbContext>((sp, opt) => UsesFargoSqlServer(sp, opt));

        public void AddFargoConnectionStringOptions(IConfiguration configuration) => services
            .AddOptions<ConnectionStringOptions>()
            .Bind(configuration.GetSection(ConnectionStringOptions.SectionName));

        public static void UsesFargoSqlServer(IServiceProvider sp, DbContextOptionsBuilder opt)
        {
            var options = sp
                .GetRequiredService<IOptions<ConnectionStringOptions>>()
                .Value;

            opt.UseSqlServer(options.Fargo);
        }

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
            .AddScoped<IPartitionQueryRepository, PartitionRepository>();

        public void AddFargoSecurity() => services
            .AddScoped<IPasswordHasher, IdentityPasswordHasher>()
            .AddScoped<ITokenGenerator, JwtTokenGenerator>()
            .AddScoped<ITokenHasher, Sha256TokenHasher>()
            .AddScoped<IRefreshTokenGenerator, CryptoRefreshTokenGenerator>();
    }
}
