using Fargo.Api.Helpers;
using Fargo.Application.Models.TreeModels;
using Fargo.Application.Queries;
using Fargo.Application.Queries.TreeQueries;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Fargo.Api.Extensions;

public static class TreeEndpointRouteBuilderExtension
{
    public static void MapFargoTree(this IEndpointRouteBuilder builder)
    {
        var group = builder
            .MapGroup("/trees")
            .RequireAuthorization()
            .WithTags("Trees");

        group.MapGet("/partitions", GetPartitionTree)
            .WithName("GetPartitionTree")
            .WithSummary("Gets partition tree members")
            .Produces<IReadOnlyCollection<EntityTreeNode>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent);

        group.MapGet("/user-groups", GetUserGroupTree)
            .WithName("GetUserGroupTree")
            .WithSummary("Gets user group tree members")
            .Produces<IReadOnlyCollection<EntityTreeNode>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent);

        group.MapGet("/articles", GetArticleTree)
            .WithName("GetArticleTree")
            .WithSummary("Gets article tree members")
            .Produces<IReadOnlyCollection<EntityTreeNode>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent);
    }

    private static async Task<Results<Ok<IReadOnlyCollection<EntityTreeNode>>, NoContent>> GetPartitionTree(
        Guid? parentPartitionGuid,
        Page? page,
        Limit? limit,
        IQueryHandler<PartitionTreeQuery, IReadOnlyCollection<EntityTreeNode>> handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.Handle(
            new PartitionTreeQuery(
                ParentPartitionGuid: parentPartitionGuid,
                Pagination: PaginationHelpers.CreatePagination(page, limit)),
            cancellationToken);

        return TypedResultsHelpers.HandleCollectionQueryResult(response);
    }

    private static async Task<Results<Ok<IReadOnlyCollection<EntityTreeNode>>, NoContent>> GetUserGroupTree(
        Guid? userGroupGuid,
        Page? page,
        Limit? limit,
        IQueryHandler<UserTreeQuery, IReadOnlyCollection<EntityTreeNode>> handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.Handle(
            new UserTreeQuery(
                UserGroupGuid: userGroupGuid,
                Pagination: PaginationHelpers.CreatePagination(page, limit)),
            cancellationToken);

        return TypedResultsHelpers.HandleCollectionQueryResult(response);
    }

    private static async Task<Results<Ok<IReadOnlyCollection<EntityTreeNode>>, NoContent>> GetArticleTree(
        Guid? articleGuid,
        Page? page,
        Limit? limit,
        IQueryHandler<ArticleTreeQuery, IReadOnlyCollection<EntityTreeNode>> handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.Handle(
            new ArticleTreeQuery(
                ArticleGuid: articleGuid,
                Pagination: PaginationHelpers.CreatePagination(page, limit)),
            cancellationToken);

        return TypedResultsHelpers.HandleCollectionQueryResult(response);
    }
}
