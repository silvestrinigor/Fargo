using Fargo.Application.Models.AuthModels;
using Fargo.Application.Persistence;
using Fargo.Application.Requests.Commands;
using Fargo.Application.Requests.Commands.ArticleCommands;
using Fargo.Application.Requests.Commands.AuthCommands;
using Fargo.Application.Requests.Commands.ItemCommands;
using Fargo.Application.Requests.Commands.PartitionCommands;
using Fargo.Application.Requests.Commands.UserCommands;
using Fargo.Application.Requests.Commands.UserGroupCommands;
using Fargo.Application.Requests.Queries;
using Fargo.Application.Requests.Queries.ArticleQueries;
using Fargo.Application.Requests.Queries.ItemQueries;
using Fargo.Application.Requests.Queries.PartitionQueries;
using Fargo.Application.Requests.Queries.UserGroupQueries;
using Fargo.Application.Requests.Queries.UserQueries;
using Fargo.Application.Security;
using Fargo.Domain.Repositories;
using Fargo.Domain.Security;
using Fargo.Domain.Services;
using Fargo.Domain.ValueObjects;
using Fargo.Infrastructure.Persistence;
using Fargo.Infrastructure.Repositories;
using Fargo.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Fargo.Infrastructure.Extensions
{
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
                AddHandlers(services);
                AddDomainServices(services);
                AddSecurity(services);
                AddPersistence(services);

                services.AddScoped<ICurrentUser, CurrentUser>();

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

                services.AddScoped<ICurrentUser, SystemCurrentUser>();

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
                services.AddScoped<IArticleRepository, ArticleRepository>();
                services.AddScoped<IItemRepository, ItemRepository>();
                services.AddScoped<IUserRepository, UserRepository>();
                services.AddScoped<IUserGroupRepository, UserGroupRepository>();
                services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
                services.AddScoped<IPartitionRepository, PartitionRepository>();
            }

            private void AddHandlers()
            {
                services.AddScoped<ICommandHandler<InitializeSystemCommand>, InitializeSystemCommandHandler>();

                services.AddScoped<ICommandHandler<LoginCommand, AuthResult>, LoginCommandHandler>();
                services.AddScoped<ICommandHandler<LogoutCommand>, LogoutCommandHandler>();
                services.AddScoped<ICommandHandler<RefreshCommand, AuthResult>, RefreshCommandHandler>();
                services.AddScoped<ICommandHandler<PasswordChangeCommand>, PasswordChangeCommandHandler>();

                services.AddScoped<ICommandHandler<ArticleCreateCommand, Guid>, ArticleCreateCommandHandler>();
                services.AddScoped<ICommandHandler<ArticleDeleteCommand>, ArticleDeleteCommandHandler>();
                services.AddScoped<ICommandHandler<ArticleUpdateCommand>, ArticleUpdateCommandHandler>();

                services.AddScoped<ICommandHandler<ItemCreateCommand, Guid>, ItemCreateCommandHandler>();
                services.AddScoped<ICommandHandler<ItemDeleteCommand>, ItemDeleteCommandHandler>();
                services.AddScoped<ICommandHandler<ItemUpdateCommand>, ItemUpdateCommandHandler>();

                services.AddScoped<ICommandHandler<UserCreateCommand, Guid>, UserCreateCommandHandler>();
                services.AddScoped<ICommandHandler<UserDeleteCommand>, UserDeleteCommandHandler>();
                services.AddScoped<ICommandHandler<UserUpdateCommand>, UserUpdateCommandHandler>();
                services.AddScoped<ICommandHandler<UserAddUserGroupCommand>, UserAddUserGroupCommandHandler>();
                services.AddScoped<ICommandHandler<UserRemoveUserGroupCommand>, UserRemoveUserGroupCommandHandler>();

                services.AddScoped<ICommandHandler<UserGroupCreateCommand, Guid>, UserGroupCreateCommandHandler>();
                services.AddScoped<ICommandHandler<UserGroupDeleteCommand>, UserGroupDeleteCommandHandler>();
                services.AddScoped<ICommandHandler<UserGroupUpdateCommand>, UserGroupUpdateCommandHandler>();

                services.AddScoped<ICommandHandler<PartitionCreateCommand, Guid>, PartitionCreateCommandHandler>();
                services.AddScoped<ICommandHandler<PartitionDeleteCommand>, PartitionDeleteCommandHandler>();
                services.AddScoped<ICommandHandler<PartitionUpdateCommand>, PartitionUpdateCommandHandler>();

                services.AddScoped<IQueryHandler<ArticleSingleQuery, ArticleInformation?>, ArticleSingleQueryHandler>();
                services.AddScoped<IQueryHandler<ArticleManyQuery, IReadOnlyCollection<ArticleInformation>>, ArticleManyQueryHandler>();

                services.AddScoped<IQueryHandler<ItemSingleQuery, ItemInformation?>, ItemSingleQueryHandler>();
                services.AddScoped<IQueryHandler<ItemManyQuery, IReadOnlyCollection<ItemInformation>>, ItemManyQueryHandler>();

                services.AddScoped<IQueryHandler<UserSingleQuery, UserInformation?>, UserSingleQueryHandler>();
                services.AddScoped<IQueryHandler<UserManyQuery, IReadOnlyCollection<UserInformation>>, UserManyQueryHandler>();

                services.AddScoped<IQueryHandler<UserGroupSingleQuery, UserGroupInformation?>, UserGroupSingleQueryHandler>();
                services.AddScoped<IQueryHandler<UserGroupManyQuery, IReadOnlyCollection<UserGroupInformation>>, UserGroupManyQueryHandler>();

                services.AddScoped<IQueryHandler<PartitionSingleQuery, PartitionInformation?>, PartitionSingleQueryHandler>();
                services.AddScoped<IQueryHandler<PartitionManyQuery, IReadOnlyCollection<PartitionInformation>>, PartitionManyQueryHandler>();
            }

            private void AddDomainServices()
            {
                services.AddScoped<ArticleService>();
                services.AddScoped<UserService>();
                services.AddScoped<UserGroupService>();
                services.AddScoped<PartitionService>();
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
                services.AddScoped<IUnitOfWork, FargoUnitOfWork>();
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
}