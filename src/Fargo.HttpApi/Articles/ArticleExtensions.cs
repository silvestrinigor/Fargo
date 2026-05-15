using Fargo.Application;
using Fargo.Application.Articles;
using Fargo.HttpApi.Contracts;
using Fargo.Sdk.Contracts.Articles;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Fargo.HttpApi.Articles;

public static class ArticleEndpointRouteBuilderExtension
{
    public static void MapFargoArticle(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapArticleGroup();

        group.MapGetArticle();

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

    #region Get Single

    private static IEndpointRouteBuilder MapGetArticle(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/{articleGuid:guid}", GetSingleArticle)
            .WithName("GetArticle")
            .WithSummary("Gets a single article")
            .WithDescription("Retrieves a single article by its unique identifier. Optionally allows querying historical data using temporal tables.")
            .Produces<ArticleInfo>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound
        );

        return builder;
    }

    private static async Task<Results<Ok<ArticleInfo>, NotFound>> GetSingleArticle(
        Guid articleGuid,
        DateTimeOffset? temporalAsOf,
        IQueryHandler<ArticleSingleQuery, ArticleDto?> handler,
        CancellationToken cancellationToken
    )
    {
        var query = new ArticleSingleQuery(articleGuid, temporalAsOf);

        var response = await handler.Handle(query, cancellationToken);

        return response is null ? TypedResults.NotFound() : TypedResults.Ok(response.ToInfo());
    }

    #endregion Get Single

    #region Get By Barcode

    private static IEndpointRouteBuilder MapGetArticleByBarcode(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/{articleBarcode:barcode}", GetArticleByBarcode)
            .WithName("GetArticleByBarcode")
            .WithSummary("Gets a single article by barcode")
            .WithDescription("Retrieves a single article by barcode and barcode type using the {barcode}:{type} route value.")
            .Produces<ArticleInfo>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        return builder;
    }

    private static async Task<Results<Ok<ArticleInfo>, NotFound>> GetArticleByBarcode(
        ArticleBarcode articleBarcode,
        DateTimeOffset? temporalAsOf,
        IQueryHandler<ArticleByBarcodeQuery, ArticleDto?> handler,
        CancellationToken cancellationToken
    )
    {
        var query = new ArticleByBarcodeQuery(articleBarcode.ToApplicationDto(), temporalAsOf);

        var response = await handler.Handle(query, cancellationToken);

        return response is null ? TypedResults.NotFound() : TypedResults.Ok(response.ToInfo());
    }

    #endregion Get By Barcode

    #region Get Many

    private static IEndpointRouteBuilder MapGetArticles(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/", GetManyArticle)
            .WithName("GetArticles")
            .WithSummary("Gets multiple articles")
            .WithDescription("Retrieves a paginated list of articles. Supports optional temporal queries and partition filters, including public articles without partitions.")
            .Produces<IReadOnlyCollection<ArticleInfo>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent
        );

        return builder;
    }

    private static async Task<Results<Ok<IReadOnlyCollection<ArticleInfo>>, NoContent>> GetManyArticle(
        DateTimeOffset? temporalAsOfDateTime,
        Page? page,
        Limit? limit,
        [FromQuery] Guid[]? childOfAnyOfThesePartitions,
        bool? notChildOfAnyPartition,
        IQueryHandler<ArticlesQuery, IReadOnlyCollection<ArticleDto>> handler,
        CancellationToken cancellationToken
    )
    {
        var withPagination = new Pagination(page ?? Page.FirstPage, limit ?? Limit.MaxLimit);

        var query = new ArticlesQuery(
            withPagination,
            temporalAsOfDateTime,
            childOfAnyOfThesePartitions,
            notChildOfAnyPartition
        );

        var response = await handler.Handle(query, cancellationToken);

        if (response.Count == 0)
        {
            return TypedResults.NoContent();
        }

        return TypedResults.Ok(response.ToInfo());
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
        ArticleCreateRequest request,
        ArticleApplicationService articles,
        CancellationToken cancellationToken)
    {
        var response = await articles.Create(
            request.ToApplicationCommand(),
            request.ToApplicationDescription(),
            request.ShelfLife,
            color: null,
            request.ToApplicationMetricsDto(),
            request.ToApplicationBarcodesDto(),
            request.Partitions,
            request.IsActive,
            cancellationToken);

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
        ArticleUpdateRequest request,
        ArticleApplicationService articles,
        CancellationToken cancellationToken)
    {
        await articles.Update(
            request.ToApplicationCommand(articleGuid),
            request.ToApplicationDescription(),
            request.ShelfLife,
            color: null,
            request.ToApplicationMetricsDto(),
            request.ToApplicationBarcodesDto(),
            request.Partitions ?? [],
            request.IsActive,
            cancellationToken);

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
        ArticleApplicationService articles,
        CancellationToken cancellationToken)
    {
        await articles.Delete(new ArticleDeleteCommand(articleGuid), cancellationToken);

        return TypedResults.NoContent();
    }

    #endregion Delete
}
