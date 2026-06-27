using Fargo.Application;
using Fargo.Application.Articles;
using Fargo.Application.Shared.Articles;
using Fargo.Core.Shared.Barcodes;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Fargo.HttpApi.Articles;

public static class ArticleEndpointRouteBuilderExtension
{
    public static void MapFargoArticle(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapArticleGroup();

        group.MapGetArticleByGuid();

        group.MapGetArticleByBarcode();

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

    #region Get By Guid

    private static IEndpointRouteBuilder MapGetArticleByGuid(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/{articleGuid:guid}", GetArticleByGuidAsync)
            .WithName("GetArticle")
            .WithSummary("Gets a single article by guid")
            .WithDescription("Retrieves a single article by its unique identifier. Optionally allows querying historical data using temporal tables.")
            .Produces<ArticleDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        return builder;
    }

    private static async Task<Results<Ok<ArticleDto>, NotFound>> GetArticleByGuidAsync(
        Guid articleGuid,
        DateTimeOffset? temporalAsOf,
        IQueryHandler<ArticleByGuidQuery, ArticleDto?> handler,
        CancellationToken cancellationToken)
    {
        var query = new ArticleByGuidQuery(articleGuid, temporalAsOf);

        var response = await handler.HandleAsync(query, cancellationToken);

        return response is null ? TypedResults.NotFound() : TypedResults.Ok(response);
    }

    #endregion

    #region Get By Barcode

    private static IEndpointRouteBuilder MapGetArticleByBarcode(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/{articleBarcode:barcode}", GetArticleByBarcodeAsync)
            .WithName("GetArticleByBarcode")
            .WithSummary("Gets a single article by barcode")
            .WithDescription("Retrieves a single article by barcode and barcode type using the {barcode}:{type} route value.")
            .Produces<ArticleDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        return builder;
    }

    private static async Task<Results<Ok<ArticleDto>, NotFound>> GetArticleByBarcodeAsync(
        Barcode articleBarcode,
        DateTimeOffset? temporalAsOf,
        IQueryHandler<ArticleByBarcodeQuery, ArticleDto?> handler,
        CancellationToken cancellationToken)
    {
        var query = new ArticleByBarcodeQuery(
            articleBarcode,
            temporalAsOf);

        var response = await handler.HandleAsync(query, cancellationToken);

        return response is null ? TypedResults.NotFound() : TypedResults.Ok(response);
    }

    #endregion

    #region Get Many

    private static IEndpointRouteBuilder MapGetArticles(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/", GetManyArticleAsync)
            .WithName("GetArticles")
            .WithSummary("Gets multiple articles")
            .WithDescription("Retrieves a paginated list of articles. Supports optional temporal queries and partition filters, including public articles without partitions.")
            .Produces<IReadOnlyCollection<ArticleDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent);

        return builder;
    }

    private static async Task<Results<Ok<IReadOnlyCollection<ArticleDto>>, NoContent>> GetManyArticleAsync(
        DateTimeOffset? temporalAsOfDateTime,
        Page? page,
        Limit? limit,
        Guid[]? childOfAnyOfThesePartitions,
        bool? notChildOfAnyPartition,
        IQueryHandler<ArticlesQuery, IReadOnlyCollection<ArticleDto>> handler,
        CancellationToken cancellationToken)
    {
        var withPagination = new Pagination(
            page ?? Page.FirstPage,
            limit ?? Limit.MaxLimit);

        var query = new ArticlesQuery(
            withPagination,
            temporalAsOfDateTime,
            childOfAnyOfThesePartitions,
            notChildOfAnyPartition
        );

        var response = await handler.HandleAsync(query, cancellationToken);

        if (response.Count == 0)
        {
            return TypedResults.NoContent();
        }

        return TypedResults.Ok(response);
    }

    #endregion

    #region Create

    private static IEndpointRouteBuilder MapCreateArticle(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("/", CreateArticleAsync)
            .WithName("CreateArticle")
            .WithSummary("Creates a new article")
            .WithDescription("Creates a new article with optional partitions, barcodes, and active state. Returns the generated identifier.")
            .Produces<Guid>(StatusCodes.Status200OK);

        return builder;
    }

    private static async Task<Ok<Guid>> CreateArticleAsync(
        ArticleCreateDto request,
        ICommandHandler<ArticleCreateCommand, Guid> handler,
        CancellationToken cancellationToken)
    {
        var articleGuid = await handler.HandleAsync(new ArticleCreateCommand(request), cancellationToken);

        return TypedResults.Ok(articleGuid);
    }

    #endregion Create

    #region Update

    private static IEndpointRouteBuilder MapUpdateArticle(this IEndpointRouteBuilder builder)
    {
        builder.MapPatch("/{articleGuid:guid}", UpdateArticleAsync)
            .WithName("PatchArticle")
            .WithSummary("Updates part of an existing article")
            .WithDescription("Updates only article fields included in the request body.")
            .Produces(StatusCodes.Status204NoContent);

        return builder;
    }

    private static async Task<NoContent> UpdateArticleAsync(
        Guid articleGuid,
        ArticleUpdateDto request,
        ICommandHandler<ArticleUpdateCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new ArticleUpdateCommand(articleGuid, request), cancellationToken);

        return TypedResults.NoContent();
    }

    #endregion Update

    #region Delete

    private static IEndpointRouteBuilder MapDeleteArticle(this IEndpointRouteBuilder builder)
    {
        builder.MapDelete("/{articleGuid:guid}", DeleteArticleAsync)
            .WithName("DeleteArticle")
            .WithSummary("Deletes an article")
            .WithDescription("Deletes the specified article from the system.")
            .Produces(StatusCodes.Status204NoContent);

        return builder;
    }

    private static async Task<NoContent> DeleteArticleAsync(
        Guid articleGuid,
        ICommandHandler<ArticleDeleteCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new ArticleDeleteCommand(articleGuid), cancellationToken);

        return TypedResults.NoContent();
    }

    #endregion Delete
}
