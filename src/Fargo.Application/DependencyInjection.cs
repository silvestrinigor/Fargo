using Fargo.Application.Articles;
using Fargo.Application.Identity;
using Fargo.Application.Items;
using Fargo.Application.Partitions;
using Fargo.Application.System;
using Fargo.Application.UserGroups;
using Fargo.Application.Users;
using Fargo.Application.Workspaces;
using Fargo.Core.Articles;
using Fargo.Core.Items;
using Fargo.Core.Partitions;
using Fargo.Core.UserGroups;
using Fargo.Core.Users;
using Microsoft.Extensions.DependencyInjection;

namespace Fargo.Application;

public static class FargoApplicationDependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddFargoApplication() => services
            .AddFargoDomain()
            .AddFargoArticleApplication()
            .AddFargoPartitionApplication()
            .AddFargoUserGroupApplication()
            .AddFargoUserApplication()
            .AddFargoItemApplication()
            .AddFargoIdentityApplication()
            .AddScoped<ICommandHandler<InitializeSystemCommand>, InitializeSystemCommandHandler>()
            .AddScoped<ICommandDispatcher, CommandDispatcher>()
            .AddScoped<IReservedGuidSession, ReservedGuidSession>()
            .AddScoped<WorkspaceApplicationService>();

        private IServiceCollection AddFargoDomain() => services
            .AddScoped<ArticleService>()
            .AddScoped<UserService>()
            .AddScoped<UserGroupService>()
            .AddScoped<PartitionService>()
            .AddScoped<ItemService>();

        private IServiceCollection AddFargoIdentityApplication() => services
            .AddScoped<ICommandHandler<LoginCommand, AuthResult>, LoginCommandHandler>()
            .AddScoped<ICommandHandler<LogoutCommand>, LogoutCommandHandler>()
            .AddScoped<ICommandHandler<RefreshCommand, AuthResult>, RefreshCommandHandler>()
            .AddScoped<ICommandHandler<PasswordChangeCommand>, PasswordChangeCommandHandler>();

        private IServiceCollection AddFargoItemApplication() => services
            .AddScoped<ItemApplicationService>()
            .AddScoped<ICommandHandler<ItemCreateCommand, Guid>, ItemCreateCommandHandler>()
            .AddScoped<ICommandHandler<ItemDeleteCommand>, ItemDeleteCommandHandler>()
            .AddScoped<ICommandHandler<ItemSetParentContainerCommand>, ItemSetParentContainerCommandHandler>()
            .AddScoped<ICommandHandler<ItemSetPartitionsCommand>, ItemSetPartitionsCommandHandler>()
            .AddScoped<ICommandHandler<ItemActivateCommand>, ItemActivateCommandHandler>()
            .AddScoped<ICommandHandler<ItemDeactivateCommand>, ItemDeactivateCommandHandler>()
            .AddScoped<IQueryHandler<ItemSingleQuery, ItemDto?>, ItemSingleQueryHandler>()
            .AddScoped<IQueryHandler<ItemsQuery, IReadOnlyCollection<ItemDto>>, ItemsQueryHandler>();

        private IServiceCollection AddFargoUserApplication() => services
            .AddScoped<UserApplicationService>()
            .AddScoped<ICommandHandler<UserCreateCommand, Guid>, UserCreateCommandHandler>()
            .AddScoped<ICommandHandler<UserDeleteCommand>, UserDeleteCommandHandler>()
            .AddScoped<ICommandHandler<UserChangeNameidCommand>, UserChangeNameidCommandHandler>()
            .AddScoped<ICommandHandler<UserChangeFirstNameCommand>, UserChangeFirstNameCommandHandler>()
            .AddScoped<ICommandHandler<UserChangeLastNameCommand>, UserChangeLastNameCommandHandler>()
            .AddScoped<ICommandHandler<UserChangeDescriptionCommand>, UserChangeDescriptionCommandHandler>()
            .AddScoped<ICommandHandler<UserSetDefaultPasswordExpirationCommand>, UserSetDefaultPasswordExpirationCommandHandler>()
            .AddScoped<ICommandHandler<UserChangePasswordCommand>, UserChangePasswordCommandHandler>()
            .AddScoped<ICommandHandler<UserSetPermissionsCommand>, UserSetPermissionsCommandHandler>()
            .AddScoped<ICommandHandler<UserSetPartitionsCommand>, UserSetPartitionsCommandHandler>()
            .AddScoped<ICommandHandler<UserSetUserGroupsCommand>, UserSetUserGroupsCommandHandler>()
            .AddScoped<ICommandHandler<UserActivateCommand>, UserActivateCommandHandler>()
            .AddScoped<ICommandHandler<UserDeactivateCommand>, UserDeactivateCommandHandler>()
            .AddScoped<IQueryHandler<UserSingleQuery, UserDto?>, UserSingleQueryHandler>()
            .AddScoped<IQueryHandler<UsersQuery, IReadOnlyCollection<UserDto>>, UsersQueryHandler>();

        private IServiceCollection AddFargoUserGroupApplication() => services
            .AddScoped<UserGroupApplicationService>()
            .AddScoped<ICommandHandler<UserGroupCreateCommand, Guid>, UserGroupCreateCommandHandler>()
            .AddScoped<ICommandHandler<UserGroupDeleteCommand>, UserGroupDeleteCommandHandler>()
            .AddScoped<ICommandHandler<UserGroupChangeNameidCommand>, UserGroupChangeNameidCommandHandler>()
            .AddScoped<ICommandHandler<UserGroupChangeDescriptionCommand>, UserGroupChangeDescriptionCommandHandler>()
            .AddScoped<ICommandHandler<UserGroupSetPermissionsCommand>, UserGroupSetPermissionsCommandHandler>()
            .AddScoped<ICommandHandler<UserGroupSetPartitionsCommand>, UserGroupSetPartitionsCommandHandler>()
            .AddScoped<ICommandHandler<UserGroupActivateCommand>, UserGroupActivateCommandHandler>()
            .AddScoped<ICommandHandler<UserGroupDeactivateCommand>, UserGroupDeactivateCommandHandler>()
            .AddScoped<IQueryHandler<UserGroupSingleQuery, UserGroupDto?>, UserGroupSingleQueryHandler>()
            .AddScoped<IQueryHandler<UserGroupsQuery, IReadOnlyCollection<UserGroupDto>>, UserGroupsQueryHandler>();

        private IServiceCollection AddFargoArticleApplication() => services
            .AddScoped<ArticleApplicationService>()
            .AddScoped<ICommandHandler<ArticleCreateCommand, Guid>, ArticleCreateCommandHandler>()
            .AddScoped<ICommandHandler<ArticleCreateVariationCommand, Guid>, ArticleCreateVariationCommandHandler>()
            .AddScoped<ICommandHandler<ArticleCreatePackCommand, Guid>, ArticleCreatePackCommandHandler>()
            .AddScoped<ICommandHandler<ArticleCreateKitCommand, Guid>, ArticleCreateKitCommandHandler>()
            .AddScoped<ICommandHandler<ArticleCreateContainerCommand, Guid>, ArticleCreateContainerCommandHandler>()
            .AddScoped<ICommandHandler<ArticleSetContainerMaxMassCommand>, ArticleSetContainerMaxMassCommandHandler>()
            .AddScoped<ICommandHandler<ArticleRenameCommand>, ArticleRenameCommandHandler>()
            .AddScoped<ICommandHandler<ArticleChangeDescriptionCommand>, ArticleChangeDescriptionCommandHandler>()
            .AddScoped<ICommandHandler<ArticleSetShelfLifeCommand>, ArticleSetShelfLifeCommandHandler>()
            .AddScoped<ICommandHandler<ArticleSetColorCommand>, ArticleSetColorCommandHandler>()
            .AddScoped<ICommandHandler<ArticleSetMetricsCommand>, ArticleSetMetricsCommandHandler>()
            .AddScoped<ICommandHandler<ArticleSetBarcodesCommand>, ArticleSetBarcodesCommandHandler>()
            .AddScoped<ICommandHandler<ArticleSetPartitionsCommand>, ArticleSetPartitionsCommandHandler>()
            .AddScoped<ICommandHandler<ArticleActivateCommand>, ArticleActivateCommandHandler>()
            .AddScoped<ICommandHandler<ArticleDeactivateCommand>, ArticleDeactivateCommandHandler>()
            .AddScoped<ICommandHandler<ArticleDeleteCommand>, ArticleDeleteCommandHandler>()
            .AddScoped<IQueryHandler<ArticleByGuidQuery, ArticleDto?>, ArticleByGuidQueryHandler>()
            .AddScoped<IQueryHandler<ArticleByBarcodeQuery, ArticleDto?>, ArticleByBarcodeQueryHandler>()
            .AddScoped<IQueryHandler<ArticlesQuery, IReadOnlyCollection<ArticleDto>>, ArticlesQueryHandler>();

        private IServiceCollection AddFargoPartitionApplication() => services
            .AddScoped<PartitionApplicationService>()
            .AddScoped<ICommandHandler<PartitionCreateCommand, Guid>, PartitionCreateCommandHandler>()
            .AddScoped<ICommandHandler<PartitionDeleteCommand>, PartitionDeleteCommandHandler>()
            .AddScoped<ICommandHandler<PartitionRenameCommand>, PartitionRenameCommandHandler>()
            .AddScoped<ICommandHandler<PartitionChangeDescriptionCommand>, PartitionChangeDescriptionCommandHandler>()
            .AddScoped<ICommandHandler<PartitionSetParentCommand>, PartitionSetParentCommandHandler>()
            .AddScoped<ICommandHandler<PartitionActivateCommand>, PartitionActivateCommandHandler>()
            .AddScoped<ICommandHandler<PartitionDeactivateCommand>, PartitionDeactivateCommandHandler>()
            .AddScoped<IQueryHandler<PartitionSingleQuery, PartitionDto?>, PartitionSingleQueryHandler>()
            .AddScoped<IQueryHandler<PartitionsQuery, IReadOnlyCollection<PartitionDto>>, PartitionsQueryHandler>();
    }
}
