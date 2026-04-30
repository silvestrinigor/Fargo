using Fargo.Application;
using Fargo.Application.ApiClients;
using Fargo.Application.Articles;
using Fargo.Application.Authentication;
using Fargo.Application.Events;
using Fargo.Application.Items;
using Fargo.Application.Partitions;
using Fargo.Application.Persistence;
using Fargo.Application.System;
using Fargo.Application.UserGroups;
using Fargo.Application.Users;
using Fargo.Domain;
using Fargo.Domain.Articles;
using Fargo.Domain.Barcodes;
using Fargo.Domain.Items;
using Fargo.Domain.Partitions;
using Fargo.Domain.Tokens;
using Fargo.Domain.Users;
using Fargo.Infrastructure.Events;
using Fargo.Infrastructure.Options;
using Fargo.Infrastructure.Persistence;
using Fargo.Infrastructure.Repositories;
using Fargo.Infrastructure.Security;
using Fargo.Infrastructure.Storage;
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
            AddImageStorage(services, configuration);

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

            services.AddScoped<IApiClientRepository, ApiClientRepository>();
            services.AddScoped<IApiClientQueryRepository, ApiClientRepository>();
            services.AddScoped<IArticleRepository, ArticleRepository>();
            services.AddScoped<IArticleQueryRepository, ArticleRepository>();
            services.AddScoped<IBarcodeRepository, BarcodeRepository>();
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

            services.AddScoped<ICommandHandler<ApiClientCreateCommand, ApiClientCreatedResult>, ApiClientCreateCommandHandler>();
            services.AddScoped<ICommandHandler<ApiClientUpdateCommand>, ApiClientUpdateCommandHandler>();
            services.AddScoped<ICommandHandler<ApiClientDeleteCommand>, ApiClientDeleteCommandHandler>();
            services.AddScoped<IQueryHandler<ApiClientSingleQuery, ApiClientInformation?>, ApiClientSingleQueryHandler>();
            services.AddScoped<IQueryHandler<ApiClientManyQuery, IReadOnlyCollection<ApiClientInformation>>, ApiClientManyQueryHandler>();

            services.AddScoped<ICommandHandler<LoginCommand, AuthResult>, LoginCommandHandler>();
            services.AddScoped<ICommandHandler<LogoutCommand>, LogoutCommandHandler>();
            services.AddScoped<ICommandHandler<RefreshCommand, AuthResult>, RefreshCommandHandler>();
            services.AddScoped<ICommandHandler<PasswordChangeCommand>, PasswordChangeCommandHandler>();

            services.AddScoped<ICommandHandler<ArticleCreateCommand, Guid>, ArticleCreateCommandHandler>();
            services.AddScoped<ICommandHandler<ArticleDeleteCommand>, ArticleDeleteCommandHandler>();
            services.AddScoped<ICommandHandler<ArticleUpdateCommand>, ArticleUpdateCommandHandler>();
            services.AddScoped<ICommandHandler<ArticleAddPartitionCommand>, ArticleAddPartitionCommandHandler>();
            services.AddScoped<ICommandHandler<ArticleRemovePartitionCommand>, ArticleRemovePartitionCommandHandler>();
            services.AddScoped<ICommandHandler<ArticleImageUploadCommand>, ArticleImageUploadCommandHandler>();
            services.AddScoped<ICommandHandler<ArticleImageDeleteCommand>, ArticleImageDeleteCommandHandler>();
            services.AddScoped<ICommandHandler<ArticleAddBarcodeCommand, Guid>, ArticleAddBarcodeCommandHandler>();
            services.AddScoped<ICommandHandler<ArticleRemoveBarcodeCommand>, ArticleRemoveBarcodeCommandHandler>();

            services.AddScoped<ICommandHandler<ItemCreateCommand, Guid>, ItemCreateCommandHandler>();
            services.AddScoped<ICommandHandler<ItemDeleteCommand>, ItemDeleteCommandHandler>();
            services.AddScoped<ICommandHandler<ItemUpdateCommand>, ItemUpdateCommandHandler>();
            services.AddScoped<ICommandHandler<ItemAddPartitionCommand>, ItemAddPartitionCommandHandler>();
            services.AddScoped<ICommandHandler<ItemRemovePartitionCommand>, ItemRemovePartitionCommandHandler>();

            services.AddScoped<ICommandHandler<UserCreateCommand, Guid>, UserCreateCommandHandler>();
            services.AddScoped<ICommandHandler<UserDeleteCommand>, UserDeleteCommandHandler>();
            services.AddScoped<ICommandHandler<UserUpdateCommand>, UserUpdateCommandHandler>();
            services.AddScoped<ICommandHandler<UserAddUserGroupCommand>, UserAddUserGroupCommandHandler>();
            services.AddScoped<ICommandHandler<UserRemoveUserGroupCommand>, UserRemoveUserGroupCommandHandler>();
            services.AddScoped<ICommandHandler<UserAddPartitionCommand>, UserAddPartitionCommandHandler>();
            services.AddScoped<ICommandHandler<UserRemovePartitionCommand>, UserRemovePartitionCommandHandler>();

            services.AddScoped<ICommandHandler<UserGroupCreateCommand, Guid>, UserGroupCreateCommandHandler>();
            services.AddScoped<ICommandHandler<UserGroupDeleteCommand>, UserGroupDeleteCommandHandler>();
            services.AddScoped<ICommandHandler<UserGroupUpdateCommand>, UserGroupUpdateCommandHandler>();
            services.AddScoped<ICommandHandler<UserGroupPartitionAddCommand>, UserGroupPartitionAddCommandHandler>();
            services.AddScoped<ICommandHandler<UserGroupPartitionRemoveCommand>, UserGroupPartitionRemoveCommandHandler>();

            services.AddScoped<ICommandHandler<PartitionCreateCommand, Guid>, PartitionCreateCommandHandler>();
            services.AddScoped<ICommandHandler<PartitionDeleteCommand>, PartitionDeleteCommandHandler>();
            services.AddScoped<ICommandHandler<PartitionUpdateCommand>, PartitionUpdateCommandHandler>();

            services.AddScoped<IQueryHandler<ArticleSingleQuery, ArticleInformation?>, ArticleSingleQueryHandler>();
            services.AddScoped<IQueryHandler<ArticleManyQuery, IReadOnlyCollection<ArticleInformation>>, ArticleManyQueryHandler>();
            services.AddScoped<IQueryHandler<ArticlePartitionsQuery, IReadOnlyCollection<PartitionInformation>?>, ArticlePartitionsQueryHandler>();
            services.AddScoped<IQueryHandler<ArticleImageQuery, ArticleImageResult?>, ArticleImageQueryHandler>();
            services.AddScoped<IQueryHandler<ArticleBarcodesQuery, IReadOnlyCollection<BarcodeInformation>?>, ArticleBarcodesQueryHandler>();

            services.AddScoped<IQueryHandler<ItemSingleQuery, ItemInformation?>, ItemSingleQueryHandler>();
            services.AddScoped<IQueryHandler<ItemManyQuery, IReadOnlyCollection<ItemInformation>>, ItemManyQueryHandler>();
            services.AddScoped<IQueryHandler<ItemPartitionsQuery, IReadOnlyCollection<PartitionInformation>?>, ItemPartitionsQueryHandler>();

            services.AddScoped<IQueryHandler<UserSingleQuery, UserInformation?>, UserSingleQueryHandler>();
            services.AddScoped<IQueryHandler<UserManyQuery, IReadOnlyCollection<UserInformation>>, UserManyQueryHandler>();
            services.AddScoped<IQueryHandler<UserPartitionsQuery, IReadOnlyCollection<PartitionInformation>?>, UserPartitionsQueryHandler>();

            services.AddScoped<IQueryHandler<UserGroupSingleQuery, UserGroupInformation?>, UserGroupSingleQueryHandler>();
            services.AddScoped<IQueryHandler<UserGroupManyQuery, IReadOnlyCollection<UserGroupInformation>>, UserGroupManyQueryHandler>();
            services.AddScoped<IQueryHandler<UserGroupPartitionsQuery, IReadOnlyCollection<PartitionInformation>?>, UserGroupPartitionsQueryHandler>();

            services.AddScoped<IQueryHandler<PartitionSingleQuery, PartitionInformation?>, PartitionSingleQueryHandler>();
            services.AddScoped<IQueryHandler<PartitionManyQuery, IReadOnlyCollection<PartitionInformation>>, PartitionManyQueryHandler>();

            services.AddScoped<IQueryHandler<EventManyQuery, IReadOnlyCollection<EventInformation>>, EventManyQueryHandler>();
        }

        private void AddDomainServices()
        {
            services.AddScoped<ArticleService>();
            services.AddScoped<UserService>();
            services.AddScoped<UserGroupService>();
            services.AddScoped<PartitionService>();
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

        private IServiceCollection AddImageStorage(IConfiguration configuration)
        {
            services
                .AddOptions<ArticleImageOptions>()
                .Bind(configuration.GetSection(ArticleImageOptions.SectionName));

            services.AddScoped<IArticleImageStorage, LocalArticleImageStorage>();

            return services;
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
