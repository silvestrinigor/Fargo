using Fargo.Application.Common;
using Fargo.Application.Operations.Articles;
using Fargo.Application.Operations.Articles.Dtos;
using Fargo.Application.Operations.Audits;
using Fargo.Application.Operations.Audits.Dtos;
using Fargo.Application.Operations.Identity;
using Fargo.Application.Operations.Identity.Dtos;
using Fargo.Application.Operations.Items;
using Fargo.Application.Operations.Items.Dtos;
using Fargo.Application.Operations.Partitions;
using Fargo.Application.Operations.Partitions.Dtos;
using Fargo.Application.Operations.ServiceAccounts;
using Fargo.Application.Operations.ServiceAccounts.Dtos;
using Fargo.Application.Operations.SupplyOrders;
using Fargo.Application.Operations.SupplyOrders.Dtos;
using Fargo.Application.Operations.System;
using Fargo.Application.Operations.UserGroups;
using Fargo.Application.Operations.UserGroups.Dtos;
using Fargo.Application.Operations.Users;
using Fargo.Application.Operations.Users.Dtos;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Actors.Users;
using Fargo.Core.Entities.Articles;
using Fargo.Core.Entities.Items;
using Fargo.Core.Entities.Partitions;
using Fargo.Core.Entities.UserGroups;
using Fargo.Core.Identity.Authentication;
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
        .AddFargoAuditLogApplication()
        .AddFargoSupplyOrderApplication();

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
        .AddFargoCommandHandler<IdentityPasswordChangeCommand, IdentityPasswordChangeCommandHandler>()
        .AddFargoCommandHandler<ServiceTokenCommand, ServiceTokenResultDto, ServiceTokenCommandHandler>()
        .AddFargoCommandHandler<ServiceAccountCreateCommand, ServiceAccountCreatedDto, ServiceAccountCreateCommandHandler>()
        .AddFargoCommandHandler<ServiceAccountUpdateCommand, ServiceAccountUpdateCommandHandler>()
        .AddFargoCommandHandler<ServiceAccountRotateSecretCommand, ServiceAccountCreatedDto, ServiceAccountRotateSecretCommandHandler>()
        .AddFargoCommandHandler<ServiceAccountDeleteCommand, ServiceAccountDeleteCommandHandler>()
        .AddFargoQueryHandler<ServiceAccountSingleQuery, ServiceAccountDto?, ServiceAccountSingleQueryHandler>()
        .AddFargoQueryHandler<ServiceAccountsQuery, PagedResult<ServiceAccountDto>, ServiceAccountsQueryHandler>();

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
        .AddFargoQueryHandler<ItemContainerInventoryByGuidQuery, IReadOnlyCollection<ItemContainerInventoryDto>?, ItemContainerInventoryQueryHandler>();

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
        .AddFargoQueryHandler<ArticleInventoryByGuidQuery, ArticleInventoryDto?, ArticleInventoryQueryHandler>();

        public IServiceCollection AddFargoPartitionApplication() => services
        .AddFargoCommandHandler<PartitionCreateCommand, Guid, PartitionCreateCommandHandler>()
        .AddFargoCommandHandler<PartitionUpdateCommand, PartitionUpdateCommandHandler>()
        .AddFargoCommandHandler<PartitionDeleteCommand, PartitionDeleteCommandHandler>()
        .AddFargoQueryHandler<PartitionSingleQuery, PartitionDto?, PartitionSingleQueryHandler>()
        .AddFargoQueryHandler<PartitionsQuery, PagedResult<PartitionDto>, PartitionsQueryHandler>();

        public IServiceCollection AddFargoAuditLogApplication() => services
        .AddFargoQueryHandler<AuditLogsQuery, PagedResult<AuditLogDto>, AuditLogsQueryHandler>()
        .AddFargoQueryHandler<AuditLogSingleQuery, AuditLogDto?, AuditLogSingleQueryHandler>();

        public IServiceCollection AddFargoSupplyOrderApplication() => services
        .AddFargoCommandHandler<SupplyOrderCreateCommand, Guid, SupplyOrderCreateCommandHandler>()
        .AddFargoCommandHandler<SupplyOrderAddLineCommand, SupplyOrderAddLineCommandHandler>()
        .AddFargoCommandHandler<SupplyOrderChangeLineQuantityCommand, SupplyOrderChangeLineQuantityCommandHandler>()
        .AddFargoCommandHandler<SupplyOrderRemoveLineCommand, SupplyOrderRemoveLineCommandHandler>()
        .AddFargoCommandHandler<SupplyOrderSetReceivingContainerCommand, SupplyOrderSetReceivingContainerCommandHandler>()
        .AddFargoCommandHandler<SupplyOrderPlaceCommand, SupplyOrderPlaceCommandHandler>()
        .AddFargoCommandHandler<SupplyOrderReceiveCommand, SupplyOrderReceiveCommandHandler>()
        .AddFargoCommandHandler<SupplyOrderCancelCommand, SupplyOrderCancelCommandHandler>()
        .AddFargoCommandHandler<SupplyOrderCloseCommand, SupplyOrderCloseCommandHandler>()
        .AddFargoQueryHandler<SupplyOrderSingleQuery, SupplyOrderDto?, SupplyOrderSingleQueryHandler>()
        .AddFargoQueryHandler<SupplyOrdersQuery, PagedResult<SupplyOrderDto>, SupplyOrdersQueryHandler>();
    }
}
