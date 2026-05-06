using Fargo.Application;
using Fargo.Application.Articles;
using Fargo.Application.Authentication;
using Fargo.Application.Events;
using Fargo.Application.Items;
using Fargo.Application.Partitions;
using Fargo.Application.System;
using Fargo.Application.UserGroups;
using Fargo.Application.Users;
using Fargo.Domain;
using Fargo.Domain.Articles;
using Fargo.Domain.Items;
using Fargo.Domain.Partitions;
using Fargo.Domain.Tokens;
using Fargo.Domain.Users;
using Fargo.Infrastructure.Events;
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
            AddDomainServices(services);

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
            services.AddScoped<IEventQueryRepository, EventRepository>();

            services.AddScoped<IArticleRepository, ArticleRepository>();
            services.AddScoped<IArticleQueryRepository, ArticleRepository>();
            services.AddScoped<IItemRepository, ItemRepository>();
            services.AddScoped<IItemQueryRepository, ItemRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserQueryRepository, UserRepository>();
            services.AddScoped<IUserGroupRepository, UserGroupRepository>();
            services.AddScoped<IUserGroupQueryRepository, UserGroupRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IPartitionRepository, PartitionRepository>();
            services.AddScoped<IPartitionQueryRepository, PartitionRepository>();
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

            services.AddScoped<ICommandHandler<UserGroupCreateCommand, Guid>, UserGroupCreateCommandHandler>();
            services.AddScoped<ICommandHandler<UserGroupDeleteCommand>, UserGroupDeleteCommandHandler>();
            services.AddScoped<ICommandHandler<UserGroupUpdateCommand>, UserGroupUpdateCommandHandler>();

            services.AddScoped<ICommandHandler<PartitionCreateCommand, Guid>, PartitionCreateCommandHandler>();
            services.AddScoped<ICommandHandler<PartitionDeleteCommand>, PartitionDeleteCommandHandler>();
            services.AddScoped<ICommandHandler<PartitionUpdateCommand>, PartitionUpdateCommandHandler>();

            services.AddScoped<IQueryHandler<ArticleSingleQuery, ArticleDto?>, ArticleSingleQueryHandler>();
            services.AddScoped<IQueryHandler<ArticlesQuery, IReadOnlyCollection<ArticleDto>>, ArticlesQueryHandler>();

            services.AddScoped<IQueryHandler<ItemSingleQuery, ItemDto?>, ItemSingleQueryHandler>();
            services.AddScoped<IQueryHandler<ItemsQuery, IReadOnlyCollection<ItemDto>>, ItemsQueryHandler>();

            services.AddScoped<IQueryHandler<UserSingleQuery, UserDto?>, UserSingleQueryHandler>();
            services.AddScoped<IQueryHandler<UsersQuery, IReadOnlyCollection<UserDto>>, UsersQueryHandler>();

            services.AddScoped<IQueryHandler<UserGroupSingleQuery, UserGroupDto?>, UserGroupSingleQueryHandler>();
            services.AddScoped<IQueryHandler<UserGroupsQuery, IReadOnlyCollection<UserGroupDto>>, UserGroupsQueryHandler>();

            services.AddScoped<IQueryHandler<PartitionSingleQuery, PartitionDto?>, PartitionSingleQueryHandler>();
            services.AddScoped<IQueryHandler<PartitionsQuery, IReadOnlyCollection<PartitionDto>>, PartitionsQueryHandler>();

            services.AddScoped<IQueryHandler<EventManyQuery, IReadOnlyCollection<EventInformation>>, EventManyQueryHandler>();
        }

        private void AddDomainServices()
        {
            services.AddScoped<UserService>();
            services.AddScoped<UserGroupService>();
            services.AddScoped<PartitionService>();
            services.AddScoped<ItemService>();
            services.AddScoped<ActorService>();
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
            services.AddScoped<IEventRecorder, DbEventRecorder>();
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
