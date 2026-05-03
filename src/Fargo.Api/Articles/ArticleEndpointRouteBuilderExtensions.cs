using Fargo.Application;
using Fargo.Application.Articles;
using Fargo.Domain;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Fargo.Api.Articles;

public static class ArticleEndpointRouteBuilderExtension
{
    public static void MapFargoArticle(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapArticleGroup();

        group.MapGetArticle();

        group.MapGetArticles();

        group.MapCreateArticle();

        group.MapUpdateArticle();

        group.MapDeleteArticle();
    }

    private static RouteGroupBuilder MapArticleGroup(this IEndpointRouteBuilder builder)
    {
        var group = builder
            .MapGroup("/articles")
            .RequireAuthorization()
            .WithTags("Articles");

        return group;
    }

    #region Get Single

    private static IEndpointRouteBuilder MapGetArticle(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/{articleGuid:guid}", GetSingleArticle)
            .WithName("GetArticle")
            .WithSummary("Gets a single article")
            .WithDescription("Retrieves a single article by its unique identifier. Optionally allows querying historical data using temporal tables.")
            .Produces<ArticleDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound
        );

        return builder;
    }

    private static async Task<Results<Ok<ArticleDto>, NotFound>> GetSingleArticle(
        Guid articleGuid,
        DateTimeOffset? temporalAsOf,
        IQueryHandler<ArticleSingleQuery, ArticleDto?> handler,
        CancellationToken cancellationToken
    )
    {
        var query = new ArticleSingleQuery(articleGuid, temporalAsOf);

        var response = await handler.Handle(query, cancellationToken);

        return response is null ? TypedResults.NotFound() : TypedResults.Ok(response);
    }

    #endregion Get Single

    #region Get Many

    private static IEndpointRouteBuilder MapGetArticles(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/", GetManyArticle)
            .WithName("GetArticles")
            .WithSummary("Gets multiple articles")
            .WithDescription("Retrieves a paginated list of articles. Supports optional temporal queries.")
            .Produces<IReadOnlyCollection<ArticleDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent
        );

        return builder;
    }

    private static async Task<Results<Ok<IReadOnlyCollection<ArticleDto>>, NoContent>> GetManyArticle(
        DateTimeOffset? temporalAsOfDateTime,
        Page? page,
        Limit? limit,
        IReadOnlyCollection<Guid>? insideAnyOfThisPartitions,
        bool? notInsideAnyPartition,
        IQueryHandler<ArticlesQuery, IReadOnlyCollection<ArticleDto>> handler,
        CancellationToken cancellationToken
    )
    {
        var withPagination = new Pagination(page ?? Page.FirstPage, limit ?? Limit.MaxLimit);

        var query = new ArticlesQuery(
            withPagination,
            temporalAsOfDateTime,
            insideAnyOfThisPartitions,
            notInsideAnyPartition
        );

        var response = await handler.Handle(query, cancellationToken);

        if (response.Count == 0)
        {
            return TypedResults.NoContent();
        }

        return TypedResults.Ok(response);
    }

    #endregion Get Many

    #region Create

    private static IEndpointRouteBuilder MapCreateArticle(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("/", CreateArticle)
            .WithName("CreateArticle")
            .WithSummary("Creates a new article")
            .WithDescription("Creates a new article with optional partitions, barcodes, and active state. Returns the generated identifier.")
            .Produces<Guid>(StatusCodes.Status200OK);

        return builder;
    }

    private static async Task<Ok<Guid>> CreateArticle(
        ArticleCreateModel request,
        ICommandHandler<ArticleCreateCommand, Guid> handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.Handle(new ArticleCreateCommand(request), cancellationToken);

        return TypedResults.Ok(response);
    }

    #endregion Create

    #region Update

    private static IEndpointRouteBuilder MapUpdateArticle(this IEndpointRouteBuilder builder)
    {
        builder.MapPut("/{articleGuid:guid}", UpdateArticle)
            .WithName("UpdateArticle")
            .WithSummary("Replaces an existing article")
            .WithDescription("Replaces all article state including partitions, barcodes, and active flag.")
            .Produces(StatusCodes.Status204NoContent);

        return builder;
    }

    private static async Task<NoContent> UpdateArticle(
        Guid articleGuid,
        ArticleUpdateDto request,
        ICommandHandler<ArticleUpdateCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(new ArticleUpdateCommand(articleGuid, request), cancellationToken);

        return TypedResults.NoContent();
    }

    #endregion Update

    #region Delete

    private static IEndpointRouteBuilder MapDeleteArticle(this IEndpointRouteBuilder builder)
    {
        builder.MapDelete("/{articleGuid:guid}", DeleteArticle)
            .WithName("DeleteArticle")
            .WithSummary("Deletes an article")
            .WithDescription("Deletes the specified article from the system.")
            .Produces(StatusCodes.Status204NoContent);

        return builder;
    }

    private static async Task<NoContent> DeleteArticle(
        Guid articleGuid,
        ICommandHandler<ArticleDeleteCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(new ArticleDeleteCommand(articleGuid), cancellationToken);

        return TypedResults.NoContent();
    }

    #endregion Delete
}
