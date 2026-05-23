using Fargo.Application.Articles;
using Fargo.Application.Identity;
using Fargo.Application.Items;
using Fargo.Application.Partitions;
using Fargo.Application.System;
using Fargo.Application.UserGroups;
using Fargo.Application.Users;
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
            .AddScoped<ICommandDispatcher, CommandDispatcher>();

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
            .AddScoped<ICommandHandler<ItemCreateCommand, Guid>, ItemCreateCommandHandler>()
            .AddScoped<ICommandHandler<ItemUpdateCommand>, ItemUpdateCommandHandler>()
            .AddScoped<ICommandHandler<ItemDeleteCommand>, ItemDeleteCommandHandler>()
            .AddScoped<IQueryHandler<ItemSingleQuery, ItemDto?>, ItemSingleQueryHandler>()
            .AddScoped<IQueryHandler<ItemsQuery, IReadOnlyCollection<ItemDto>>, ItemsQueryHandler>();

        private IServiceCollection AddFargoUserApplication() => services
            .AddScoped<ICommandHandler<UserCreateCommand, Guid>, UserCreateCommandHandler>()
            .AddScoped<ICommandHandler<UserUpdateCommand>, UserUpdateCommandHandler>()
            .AddScoped<ICommandHandler<UserDeleteCommand>, UserDeleteCommandHandler>()
            .AddScoped<IQueryHandler<UserSingleQuery, UserDto?>, UserSingleQueryHandler>()
            .AddScoped<IQueryHandler<UsersQuery, IReadOnlyCollection<UserDto>>, UsersQueryHandler>();

        private IServiceCollection AddFargoUserGroupApplication() => services
            .AddScoped<ICommandHandler<UserGroupCreateCommand, Guid>, UserGroupCreateCommandHandler>()
            .AddScoped<ICommandHandler<UserGroupUpdateCommand>, UserGroupUpdateCommandHandler>()
            .AddScoped<ICommandHandler<UserGroupDeleteCommand>, UserGroupDeleteCommandHandler>()
            .AddScoped<IQueryHandler<UserGroupSingleQuery, UserGroupDto?>, UserGroupSingleQueryHandler>()
            .AddScoped<IQueryHandler<UserGroupsQuery, IReadOnlyCollection<UserGroupDto>>, UserGroupsQueryHandler>();

        private IServiceCollection AddFargoArticleApplication() => services
            .AddScoped<ICommandHandler<ArticleCreateCommand, Guid>, ArticleCreateCommandHandler>()
            .AddScoped<ICommandHandler<ArticlePatchCommand>, ArticlePatchCommandHandler>()
            .AddScoped<ICommandHandler<ArticleDeleteCommand>, ArticleDeleteCommandHandler>()
            .AddScoped<IQueryHandler<ArticleByGuidQuery, ArticleDto?>, ArticleByGuidQueryHandler>()
            .AddScoped<IQueryHandler<ArticleByBarcodeQuery, ArticleDto?>, ArticleByBarcodeQueryHandler>()
            .AddScoped<IQueryHandler<ArticlesQuery, IReadOnlyCollection<ArticleDto>>, ArticlesQueryHandler>();

        private IServiceCollection AddFargoPartitionApplication() => services
            .AddScoped<ICommandHandler<PartitionCreateCommand, Guid>, PartitionCreateCommandHandler>()
            .AddScoped<ICommandHandler<PartitionUpdateCommand>, PartitionUpdateCommandHandler>()
            .AddScoped<ICommandHandler<PartitionDeleteCommand>, PartitionDeleteCommandHandler>()
            .AddScoped<IQueryHandler<PartitionSingleQuery, PartitionDto?>, PartitionSingleQueryHandler>()
            .AddScoped<IQueryHandler<PartitionsQuery, IReadOnlyCollection<PartitionDto>>, PartitionsQueryHandler>();
    }
}
