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

            services.AddRepositories();

            services.AddSecurity();

            options.ConfigureCurrentUser?.Invoke(services);

            return services;
        }

        public IServiceCollection AddFargoJwt(
                IConfiguration configuration)
        {
            services
                .AddOptions<JwtOptions>()
                .Bind(configuration.GetSection(JwtOptions.SectionName))
                .ValidateDataAnnotations();

            return services;
        }

        private IServiceCollection AddFargoUnitOfWork() => services
            .AddScoped<IUnitOfWork, UnitOfWork>();

        private IServiceCollection AddFargoDbContext() => services
            .AddDbContext<FargoDbContext>((sp, opt) => UsesFargoSqlServer(sp, opt));

        private void AddFargoConnectionStringOptions(IConfiguration configuration) => services
            .AddOptions<ConnectionStringOptions>()
            .Bind(configuration.GetSection(ConnectionStringOptions.SectionName));

        private static void UsesFargoSqlServer(IServiceProvider sp, DbContextOptionsBuilder opt)
        {
            var options = sp
                .GetRequiredService<IOptions<ConnectionStringOptions>>()
                .Value;

            opt.UseSqlServer(options.Fargo);
        }

        private void AddRepositories() => services
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

        private void AddSecurity() => services
            .AddScoped<IPasswordHasher, IdentityPasswordHasher>()
            .AddScoped<ITokenGenerator, JwtTokenGenerator>()
            .AddScoped<ITokenHasher, Sha256TokenHasher>()
            .AddScoped<IRefreshTokenGenerator, CryptoRefreshTokenGenerator>();
    }
}
