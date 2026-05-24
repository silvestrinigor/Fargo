using Fargo.Application;
using Fargo.Application.Articles;
using Fargo.Core.Articles;
using Fargo.Core.Barcodes;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

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
        IQueryHandler<ArticleByGuidQuery, ArticleDto?> handler,
        CancellationToken cancellationToken
    )
    {
        var query = new ArticleByGuidQuery(articleGuid, temporalAsOf);

        var response = await handler.Handle(query, cancellationToken);

        return response is null ? TypedResults.NotFound() : TypedResults.Ok(response);
    }

    #endregion

    #region Get By Barcode

    private static IEndpointRouteBuilder MapGetArticleByBarcode(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/{articleBarcode:barcode}", GetArticleByBarcode)
            .WithName("GetArticleByBarcode")
            .WithSummary("Gets a single article by barcode")
            .WithDescription("Retrieves a single article by barcode and barcode type using the {barcode}:{type} route value.")
            .Produces<ArticleDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        return builder;
    }

    private static async Task<Results<Ok<ArticleDto>, NotFound>> GetArticleByBarcode(
        ArticleBarcodeRouteValue articleBarcode,
        DateTimeOffset? temporalAsOf,
        IQueryHandler<ArticleByBarcodeQuery, ArticleDto?> handler,
        CancellationToken cancellationToken
    )
    {
        var query = new ArticleByBarcodeQuery(articleBarcode.ToDomainBarcode(), temporalAsOf);

        var response = await handler.Handle(query, cancellationToken);

        return response is null ? TypedResults.NotFound() : TypedResults.Ok(response);
    }

    #endregion

    #region Get Many

    private static IEndpointRouteBuilder MapGetArticles(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/", GetManyArticle)
            .WithName("GetArticles")
            .WithSummary("Gets multiple articles")
            .WithDescription("Retrieves a paginated list of articles. Supports optional temporal queries and partition filters, including public articles without partitions.")
            .Produces<IReadOnlyCollection<ArticleDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent
        );

        return builder;
    }

    private static async Task<Results<Ok<IReadOnlyCollection<ArticleDto>>, NoContent>> GetManyArticle(
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

        return TypedResults.Ok(response);
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
        ArticleCreateDto request,
        ICommandHandler<ArticleCreateDefaultCommand, Guid> defaultHandler,
        ICommandHandler<ArticleCreateVariationCommand, Guid> variationHandler,
        ICommandHandler<ArticleCreatePackCommand, Guid> packHandler,
        ICommandHandler<ArticleCreateKitCommand, Guid> kitHandler,
        ICommandHandler<ArticleCreateContainerCommand, Guid> containerHandler,
        CancellationToken cancellationToken)
    {
        var response = request.ArticleType switch
        {
            ArticleType.Default => await defaultHandler.Handle(
                CreateDefaultCommand(request),
                cancellationToken),
            ArticleType.Variation => await variationHandler.Handle(
                CreateVariationCommand(request),
                cancellationToken),
            ArticleType.Pack => await packHandler.Handle(
                CreatePackCommand(request),
                cancellationToken),
            ArticleType.Kit => await kitHandler.Handle(
                CreateKitCommand(request),
                cancellationToken),
            ArticleType.Container => await containerHandler.Handle(
                CreateContainerCommand(request),
                cancellationToken),
            _ => throw new ArgumentException(
                $"Unsupported article type '{request.ArticleType}'.",
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
        ArticlePatchDto request,
        ICommandHandler<ArticlePatchCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(new ArticlePatchCommand(articleGuid, request), cancellationToken);

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

/// <summary>Represents an article barcode route value in the format <c>{barcode}:{type}</c>.</summary>
public readonly record struct ArticleBarcodeRouteValue(string Barcode, BarcodeFormat Type)
    : IParsable<ArticleBarcodeRouteValue>
{
    public override string ToString() => $"{Barcode}:{Type}";

    public Barcode ToDomainBarcode() => new(Barcode, Type);

    public static ArticleBarcodeRouteValue Parse(string s, IFormatProvider? provider)
    {
        if (TryParse(s, provider, out var result))
        {
            return result;
        }

        throw new FormatException($"Invalid article barcode value: '{s}'. Expected '{{barcode}}:{{type}}'.");
    }

    public static bool TryParse(string? s, IFormatProvider? provider, out ArticleBarcodeRouteValue result)
    {
        result = default;

        if (string.IsNullOrWhiteSpace(s))
        {
            return false;
        }

        var separator = s.LastIndexOf(':');
        if (separator <= 0 || separator == s.Length - 1)
        {
            return false;
        }

        var barcode = s[..separator];
        var typeText = s[(separator + 1)..];

        if (string.IsNullOrWhiteSpace(barcode) ||
            !Enum.TryParse<BarcodeFormat>(typeText, ignoreCase: true, out var type))
        {
            return false;
        }

        result = new ArticleBarcodeRouteValue(barcode, type);
        return true;
    }
}
