using Fargo.Application;
using Fargo.Application.Articles;
using Fargo.Application.Shared.Articles;
using Fargo.Core.Shared.Articles;
using Fargo.Core.Shared.Barcodes;
using Fargo.HttpApi.Contracts;
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
        builder.MapGet("/{articleGuid:guid}", GetArticleByGuid)
            .WithName("GetArticle")
            .WithSummary("Gets a single article by guid")
            .WithDescription("Retrieves a single article by its unique identifier. Optionally allows querying historical data using temporal tables.")
            .Produces<HttpContracts.ArticleDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound
        );

        return builder;
    }

    private static async Task<Results<Ok<HttpContracts.ArticleDto>, NotFound>> GetArticleByGuid(
        Guid articleGuid,
        DateTimeOffset? temporalAsOf,
        IQueryHandler<ArticleByGuidQuery, ArticleDto?> handler,
        CancellationToken cancellationToken
    )
    {
        var query = new ArticleByGuidQuery(articleGuid, temporalAsOf);

        var response = await handler.Handle(query, cancellationToken);

        return response is null ? TypedResults.NotFound() : TypedResults.Ok(response.ToContract());
    }

    #endregion

    #region Get By Barcode

    private static IEndpointRouteBuilder MapGetArticleByBarcode(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/{articleBarcode:barcode}", GetArticleByBarcode)
            .WithName("GetArticleByBarcode")
            .WithSummary("Gets a single article by barcode")
            .WithDescription("Retrieves a single article by barcode and barcode type using the {barcode}:{type} route value.")
            .Produces<HttpContracts.ArticleDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        return builder;
    }

    private static async Task<Results<Ok<HttpContracts.ArticleDto>, NotFound>> GetArticleByBarcode(
        Barcode articleBarcode,
        DateTimeOffset? temporalAsOf,
        IQueryHandler<ArticleByBarcodeQuery, ArticleDto?> handler,
        CancellationToken cancellationToken
    )
    {
        var query = new ArticleByBarcodeQuery(
            articleBarcode,
            temporalAsOf);

        var response = await handler.Handle(query, cancellationToken);

        return response is null ? TypedResults.NotFound() : TypedResults.Ok(response.ToContract());
    }

    #endregion

    #region Get Many

    private static IEndpointRouteBuilder MapGetArticles(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/", GetManyArticle)
            .WithName("GetArticles")
            .WithSummary("Gets multiple articles")
            .WithDescription("Retrieves a paginated list of articles. Supports optional temporal queries and partition filters, including public articles without partitions.")
            .Produces<IReadOnlyCollection<HttpContracts.ArticleDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent
        );

        return builder;
    }

    private static async Task<Results<Ok<IReadOnlyCollection<HttpContracts.ArticleDto>>, NoContent>> GetManyArticle(
        DateTimeOffset? temporalAsOfDateTime,
        Page? page,
        Limit? limit,
        Guid[]? childOfAnyOfThesePartitions,
        bool? notChildOfAnyPartition,
        IQueryHandler<ArticlesQuery, IReadOnlyCollection<ArticleDto>> handler,
        CancellationToken cancellationToken
    )
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

        var response = await handler.Handle(query, cancellationToken);

        if (response.Count == 0)
        {
            return TypedResults.NoContent();
        }

        IReadOnlyCollection<HttpContracts.ArticleDto> contractResponse =
            [.. response.Select(static article => article.ToContract())];

        return TypedResults.Ok(contractResponse);
    }

    #endregion

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
        HttpContracts.ArticleCreateRequest request,
        ICommandHandler<ArticleCreateDefaultCommand, Guid> defaultHandler,
        ICommandHandler<ArticleCreateVariationCommand, Guid> variationHandler,
        ICommandHandler<ArticleCreatePackCommand, Guid> packHandler,
        ICommandHandler<ArticleCreateKitCommand, Guid> kitHandler,
        ICommandHandler<ArticleCreateContainerCommand, Guid> containerHandler,
        CancellationToken cancellationToken)
    {
        var create = request.ToApplication();

        var response = create.ArticleType switch
        {
            ArticleType.Default => await defaultHandler.Handle(
                CreateDefaultCommand(create),
                cancellationToken),
            ArticleType.Variation => await variationHandler.Handle(
                CreateVariationCommand(create),
                cancellationToken),
            ArticleType.Pack => await packHandler.Handle(
                CreatePackCommand(create),
                cancellationToken),
            ArticleType.Kit => await kitHandler.Handle(
                CreateKitCommand(create),
                cancellationToken),
            ArticleType.Container => await containerHandler.Handle(
                CreateContainerCommand(create),
                cancellationToken),
            _ => throw new ArgumentException(
                $"Unsupported article type '{create.ArticleType}'.",
                nameof(request))
        };

        return TypedResults.Ok(response);
    }

    internal static ArticleCreateDefaultCommand CreateDefaultCommand(ArticleCreateDto request)
    {
        RejectPayload(
            request.Variation is not null,
            request.Pack is not null,
            request.Kit is not null,
            request.Container is not null,
            request.ArticleType);

        return new ArticleCreateDefaultCommand(
            request.Name,
            Description: request.Description,
            ShelfLife: request.ShelfLife,
            Color: request.Color,
            Metrics: request.Metrics,
            Barcodes: request.Barcodes,
            Partitions: request.Partitions,
            IsActive: request.IsActive);
    }

    internal static ArticleCreateVariationCommand CreateVariationCommand(ArticleCreateDto request)
    {
        RequirePayload(request.Variation is not null, request.ArticleType);
        RejectPayload(
            false,
            request.Pack is not null,
            request.Kit is not null,
            request.Container is not null,
            request.ArticleType);

        return new ArticleCreateVariationCommand(
            request.Name,
            request.Variation!.FromArticleGuid,
            Description: request.Description,
            ShelfLife: request.ShelfLife,
            Color: request.Color,
            Metrics: request.Metrics,
            Barcodes: request.Barcodes,
            Partitions: request.Partitions,
            IsActive: request.IsActive);
    }

    internal static ArticleCreatePackCommand CreatePackCommand(ArticleCreateDto request)
    {
        RequirePayload(request.Pack is not null, request.ArticleType);
        RejectPayload(
            request.Variation is not null,
            false,
            request.Kit is not null,
            request.Container is not null,
            request.ArticleType);

        return new ArticleCreatePackCommand(
            request.Name,
            request.Pack!.FromArticleGuid,
            request.Pack.Quantity,
            Description: request.Description,
            ShelfLife: request.ShelfLife,
            Color: request.Color,
            Metrics: request.Metrics,
            Barcodes: request.Barcodes,
            Partitions: request.Partitions,
            IsActive: request.IsActive);
    }

    internal static ArticleCreateKitCommand CreateKitCommand(ArticleCreateDto request)
    {
        RequirePayload(request.Kit is not null, request.ArticleType);
        RejectPayload(
            request.Variation is not null,
            request.Pack is not null,
            false,
            request.Container is not null,
            request.ArticleType);

        return new ArticleCreateKitCommand(
            request.Name,
            request.Kit!.Packs,
            Description: request.Description,
            ShelfLife: request.ShelfLife,
            Color: request.Color,
            Metrics: request.Metrics,
            Barcodes: request.Barcodes,
            Partitions: request.Partitions,
            IsActive: request.IsActive);
    }

    internal static ArticleCreateContainerCommand CreateContainerCommand(ArticleCreateDto request)
    {
        RejectPayload(
            request.Variation is not null,
            request.Pack is not null,
            request.Kit is not null,
            false,
            request.ArticleType);

        return new ArticleCreateContainerCommand(
            request.Name,
            MaxMass: request.Container?.MaxMass,
            Description: request.Description,
            ShelfLife: request.ShelfLife,
            Color: request.Color,
            Metrics: request.Metrics,
            Barcodes: request.Barcodes,
            Partitions: request.Partitions,
            IsActive: request.IsActive);
    }

    private static void RequirePayload(
        bool payloadProvided,
        ArticleType articleType)
    {
        if (!payloadProvided)
        {
            throw new ArgumentException(
                $"Article type '{articleType}' requires its matching create payload.",
                nameof(articleType));
        }
    }

    private static void RejectPayload(
        bool variationProvided,
        bool packProvided,
        bool kitProvided,
        bool containerProvided,
        ArticleType articleType)
    {
        if (variationProvided || packProvided || kitProvided || containerProvided)
        {
            throw new ArgumentException(
                $"Article type '{articleType}' cannot include payloads for another article type.",
                nameof(articleType));
        }
    }

    #endregion Create

    #region Update

    private static IEndpointRouteBuilder MapUpdateArticle(this IEndpointRouteBuilder builder)
    {
        builder.MapPatch("/{articleGuid:guid}", UpdateArticle)
            .WithName("PatchArticle")
            .WithSummary("Updates part of an existing article")
            .WithDescription("Updates only article fields included in the request body.")
            .Produces(StatusCodes.Status204NoContent);

        return builder;
    }

    private static async Task<NoContent> UpdateArticle(
        Guid articleGuid,
        HttpContracts.ArticlePatchRequest request,
        ICommandHandler<ArticlePatchCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(new ArticlePatchCommand(articleGuid, request.ToApplication()), cancellationToken);

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
