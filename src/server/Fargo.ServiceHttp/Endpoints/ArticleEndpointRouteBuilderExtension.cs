using Fargo.Application.Articles;
using Fargo.Application.Common;
using Fargo.Core.Barcodes;
using Fargo.ServiceHttp.Versioning;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Fargo.ServiceHttp.Endpoints;

public static class ArticleEndpointRouteBuilderExtension
{
    public static void MapFargoArticle(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapArticleGroup();

        group.MapGetArticleByGuid();

        group.MapGetArticleByBarcode();

        group.MapGetArticleInventoryByGuid();

        group.MapGetArticles();

        group.MapCreateArticle();

        group.MapUpdateArticle();

        group.MapDeleteArticle();
    }

    private static RouteGroupBuilder MapArticleGroup(this IEndpointRouteBuilder builder)
    {
        var group = builder
        .MapVersionedGroup("Articles", "/articles")
        .RequireAuthorization()
        .WithTags("Articles")
        .RequireRateLimiting("general-authenticated");

        return group;
    }

    #region Get By Guid

    private static IEndpointRouteBuilder MapGetArticleByGuid(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/{articleGuid:guid}", GetArticleByGuidAsync)
            .WithName("GetArticle")
            .WithSummary("Gets a single article by guid")
            .WithDescription("Retrieves a single article by its unique identifier.")
            .Produces<ArticleDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        return builder;
    }

    private static async Task<Results<Ok<ArticleDto>, NotFound>> GetArticleByGuidAsync(
        Guid articleGuid,
        IQueryHandler<ArticleByGuidQuery, ArticleDto?> handler,
        CancellationToken cancellationToken)
    {
        var query = new ArticleByGuidQuery(articleGuid);

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
        IQueryHandler<ArticleByBarcodeQuery, ArticleDto?> handler,
        CancellationToken cancellationToken)
    {
        var query = new ArticleByBarcodeQuery(articleBarcode);

        var response = await handler.HandleAsync(query, cancellationToken);

        return response is null ? TypedResults.NotFound() : TypedResults.Ok(response);
    }

    #endregion

    #region Get Inventory By Guid

    private static IEndpointRouteBuilder MapGetArticleInventoryByGuid(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/{articleGuid:guid}/inventory", GetArticleInventoryByGuidAsync)
            .WithName("GetArticleInventory")
            .WithSummary("Gets a article inventory by guid")
            .WithDescription("Retrieves the inventory count for an article. When 'at' is supplied, the count is reconstructed from the item containment history as it was at that instant.")
            .Produces<ArticleInventoryDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        return builder;
    }

    private static async Task<Results<Ok<ArticleInventoryDto>, NotFound>> GetArticleInventoryByGuidAsync(
        Guid articleGuid,
        Guid[]? insideItemContainerGuids,
        bool? includeDescendents,
        DateTimeOffset? at,
        IQueryHandler<ArticleInventoryByGuidQuery, ArticleInventoryDto?> handler,
        CancellationToken cancellationToken)
    {
        var query = new ArticleInventoryByGuidQuery(articleGuid, insideItemContainerGuids, includeDescendents ?? true, at);

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
            .WithDescription("Retrieves a paginated list of articles, including the total number of matching entries.")
            .Produces<PagedResult<ArticleDto>>(StatusCodes.Status200OK);

        return builder;
    }

    private static async Task<Ok<PagedResult<ArticleDto>>> GetManyArticleAsync(
        Page? page, Limit? limit,
        Guid[]? childOfAnyOfThesePartitions,
        IQueryHandler<ArticlesQuery, PagedResult<ArticleDto>> handler,
        CancellationToken cancellationToken)
    {
        var withPagination = new Pagination(
            page ?? Page.FirstPage, limit ?? Limit.MaxLimit);

        var query = new ArticlesQuery(
            withPagination,
            childOfAnyOfThesePartitions);

        var response = await handler.HandleAsync(query, cancellationToken);

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
        var command = new ArticleCreateCommand(request);

        var articleGuid = await handler.HandleAsync(command, cancellationToken);

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
        var command = new ArticleUpdateCommand(articleGuid, request);

        await handler.HandleAsync(command, cancellationToken);

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
        var command = new ArticleDeleteCommand(articleGuid);

        await handler.HandleAsync(command, cancellationToken);

        return TypedResults.NoContent();
    }

    #endregion Delete
}
