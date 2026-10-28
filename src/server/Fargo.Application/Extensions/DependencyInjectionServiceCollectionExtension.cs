using Fargo.Application.Articles;
using Fargo.Application.Audits;
using Fargo.Application.Common;
using Fargo.Application.Identity;
using Fargo.Application.Items;
using Fargo.Application.Partitions;
using Fargo.Application.System;
using Fargo.Application.UserGroups;
using Fargo.Application.Users;
using Fargo.Core.Actors;
using Fargo.Core.Articles;
using Fargo.Core.Identity;
using Fargo.Core.Items;
using Fargo.Core.Partitions;
using Fargo.Core.UserGroups;
using Fargo.Core.Users;
using Microsoft.Extensions.DependencyInjection;

namespace Fargo.Application.Extensions;

public static class DependencyInjectionServiceCollectionExtension
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddFargoApplication() => services
        .AddFargoCore()
        .AddFargoArticleApplication()
        .AddFargoPartitionApplication()
        .AddFargoUserGroupApplication()
        .AddFargoUserApplication()
        .AddFargoItemApplication()
        .AddFargoIdentityApplication()
        .AddFargoSystemApplication()
        .AddFargoAuditLogApplication();

        public IServiceCollection AddFargoCore() => services
        .AddScoped<ActorResolver>()
        .AddScoped<ArticleService>()
        .AddScoped<UserService>()
        .AddScoped<UserGroupService>()
        .AddScoped<PartitionService>()
        .AddScoped<ItemService>()
        .AddScoped<IAuthenticationAttemptService, AuthenticationAttemptService>();

        public IServiceCollection AddFargoSystemApplication() => services
        .AddFargoCommandHandler<InitializeSystemCommand, InitializeSystemCommandHandler>();

        public IServiceCollection AddFargoIdentityApplication() => services
        .AddFargoCommandHandler<IdentityLoginCommand, IdentityAuthResultDto, IdentityLoginCommandHandler>()
        .AddFargoCommandHandler<IdentityLogoutCommand, IdentityLogoutCommandHandler>()
        .AddFargoCommandHandler<IdentityRefreshCommand, IdentityAuthResultDto, IdentityRefreshCommandHandler>()
        .AddFargoCommandHandler<IdentityPasswordChangeCommand, IdentityPasswordChangeCommandHandler>();

        public IServiceCollection AddFargoItemApplication() => services
        .AddFargoCommandHandler<ItemCreateCommand, Guid, ItemCreateCommandHandler>()
        .AddFargoCommandHandler<ItemUpdateCommand, ItemUpdateCommandHandler>()
        .AddFargoCommandHandler<ItemDeleteCommand, ItemDeleteCommandHandler>()
        .AddFargoCommandHandler<ItemEventCreateCommand, Guid, ItemEventCreateCommandHandler>()
        .AddFargoQueryHandler<ItemSingleQuery, ItemDto?, ItemSingleQueryHandler>()
        .AddFargoQueryHandler<ItemsQuery, PagedResult<ItemDto>, ItemsQueryHandler>()
        .AddFargoQueryHandler<ItemLocationQuery, IReadOnlyCollection<ItemDto>, ItemLocationQueryHandler>()
        .AddFargoQueryHandler<ItemMovimentsQuery, IReadOnlyCollection<ItemMovimentDto>?, ItemMovimentsQueryHandler>()
        .AddFargoQueryHandler<ItemEventsQuery, PagedResult<ItemEventDto>, ItemEventsQueryHandler>()
        .AddFargoQueryHandler<ItemContainerInventoryByGuidQuery, IReadOnlyCollection<ItemContainerInventoryDto>?, ItemContainerInventoryByGuidQueryHandler>();

        public IServiceCollection AddFargoUserApplication() => services
        .AddFargoCommandHandler<UserCreateCommand, Guid, UserCreateCommandHandler>()
        .AddFargoCommandHandler<UserUpdateCommand, UserUpdateCommandHandler>()
        .AddFargoCommandHandler<UserDeleteCommand, UserDeleteCommandHandler>()
        .AddFargoQueryHandler<UserSingleQuery, UserDto?, UserSingleQueryHandler>()
        .AddFargoQueryHandler<UsersQuery, PagedResult<UserDto>, UsersQueryHandler>();

        public IServiceCollection AddFargoUserGroupApplication() => services
        .AddFargoCommandHandler<UserGroupCreateCommand, Guid, UserGroupCreateCommandHandler>()
        .AddFargoCommandHandler<UserGroupUpdateCommand, UserGroupUpdateCommandHandler>()
        .AddFargoCommandHandler<UserGroupDeleteCommand, UserGroupDeleteCommandHandler>()
        .AddFargoQueryHandler<UserGroupSingleQuery, UserGroupDto?, UserGroupSingleQueryHandler>()
        .AddFargoQueryHandler<UserGroupsQuery, PagedResult<UserGroupDto>, UserGroupsQueryHandler>();

        public IServiceCollection AddFargoArticleApplication() => services
        .AddFargoCommandHandler<ArticleCreateCommand, Guid, ArticleCreateCommandHandler>()
        .AddFargoCommandHandler<ArticleUpdateCommand, ArticleUpdateCommandHandler>()
        .AddFargoCommandHandler<ArticleDeleteCommand, ArticleDeleteCommandHandler>()
        .AddFargoQueryHandler<ArticleByGuidQuery, ArticleDto?, ArticleQueryHandler>()
        .AddFargoQueryHandler<ArticleByBarcodeQuery, ArticleDto?, ArticleQueryHandler>()
        .AddFargoQueryHandler<ArticlesQuery, PagedResult<ArticleDto>, ArticlesQueryHandler>()
        .AddFargoQueryHandler<ArticleInventoryByGuidQuery, ArticleInventoryDto?, ArticleInventoryByGuidQueryHandler>();

        public IServiceCollection AddFargoPartitionApplication() => services
        .AddFargoCommandHandler<PartitionCreateCommand, Guid, PartitionCreateCommandHandler>()
        .AddFargoCommandHandler<PartitionUpdateCommand, PartitionUpdateCommandHandler>()
        .AddFargoCommandHandler<PartitionDeleteCommand, PartitionDeleteCommandHandler>()
        .AddFargoQueryHandler<PartitionSingleQuery, PartitionDto?, PartitionSingleQueryHandler>()
        .AddFargoQueryHandler<PartitionsQuery, PagedResult<PartitionDto>, PartitionsQueryHandler>();

        public IServiceCollection AddFargoAuditLogApplication() => services
        .AddFargoQueryHandler<AuditLogsQuery, PagedResult<AuditLogDto>, AuditLogsQueryHandler>()
        .AddFargoQueryHandler<AuditLogSingleQuery, AuditLogDto?, AuditLogSingleQueryHandler>();
    }
}
