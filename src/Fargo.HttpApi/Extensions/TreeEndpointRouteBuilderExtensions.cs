using Fargo.Application.Models.TreeModels;
using Fargo.Application.Queries;
using Fargo.Application.Queries.TreeQueries;
using Fargo.Domain.ValueObjects;
using Fargo.HttpApi.Helpers;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Fargo.HttpApi.Extensions;

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
            .Produces<IReadOnlyCollection<TreeNode>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent);

        group.MapGet("/user-groups", GetUserGroupTree)
            .WithName("GetUserGroupTree")
            .WithSummary("Gets user group tree members")
            .Produces<IReadOnlyCollection<TreeNode>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent);

        group.MapGet("/partitions/security", GetPartitionSecurityTree)
            .WithName("GetPartitionSecurityTree")
            .WithSummary("Gets partition security tree members")
            .Produces<IReadOnlyCollection<TreeNode>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent);

        group.MapGet("/articles", GetArticleTree)
            .WithName("GetArticleTree")
            .WithSummary("Gets article tree members")
            .Produces<IReadOnlyCollection<TreeNode>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent);
    }

    private static async Task<Results<Ok<IReadOnlyCollection<TreeNode>>, NoContent>> GetPartitionTree(
        Guid? parentPartitionGuid,
        Page? page,
        Limit? limit,
        IQueryHandler<PartitionTreeQuery, IReadOnlyCollection<TreeNode>> handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.Handle(
            new PartitionTreeQuery(
                ParentPartitionGuid: parentPartitionGuid,
                Pagination: PaginationHelpers.CreatePagination(page, limit)),
            cancellationToken);

        return TypedResultsHelpers.HandleCollectionQueryResult(response);
    }

    private static async Task<Results<Ok<IReadOnlyCollection<TreeNode>>, NoContent>> GetUserGroupTree(
        Guid? userGroupGuid,
        Page? page,
        Limit? limit,
        IQueryHandler<UserGroupTreeQuery, IReadOnlyCollection<TreeNode>> handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.Handle(
            new UserGroupTreeQuery(
                UserGroupGuid: userGroupGuid,
                Pagination: PaginationHelpers.CreatePagination(page, limit)),
            cancellationToken);

        return TypedResultsHelpers.HandleCollectionQueryResult(response);
    }

    private static async Task<Results<Ok<IReadOnlyCollection<TreeNode>>, NoContent>> GetPartitionSecurityTree(
        Guid? partitionGuid,
        Page? page,
        Limit? limit,
        IQueryHandler<PartitionSecurityTreeQuery, IReadOnlyCollection<TreeNode>> handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.Handle(
            new PartitionSecurityTreeQuery(
                PartitionGuid: partitionGuid,
                Pagination: PaginationHelpers.CreatePagination(page, limit)),
            cancellationToken);

        return TypedResultsHelpers.HandleCollectionQueryResult(response);
    }

    private static async Task<Results<Ok<IReadOnlyCollection<TreeNode>>, NoContent>> GetArticleTree(
        Guid? articleGuid,
        Page? page,
        Limit? limit,
        IQueryHandler<ArticleTreeQuery, IReadOnlyCollection<TreeNode>> handler,
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
