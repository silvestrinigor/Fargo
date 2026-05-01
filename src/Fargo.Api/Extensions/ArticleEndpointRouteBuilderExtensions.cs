using Fargo.Api.Contracts;
using Fargo.Sdk.Contracts.Articles;
using Fargo.Sdk.Contracts.Partitions;
using Fargo.Api.Helpers;
using Fargo.Application;
using Fargo.Application.Articles;
using Fargo.Application.Partitions;
using Fargo.Domain;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Fargo.Api.Extensions;

/// <summary>
/// Extension responsible for mapping all Article endpoints.
/// </summary>
public static class ArticleEndpointRouteBuilderExtension
{
    /// <summary>
    /// Maps all routes related to articles.
    /// </summary>
    /// <param name="builder">The endpoint route builder.</param>
    public static void MapFargoArticle(this IEndpointRouteBuilder builder)
    {
        var group = builder
            .MapGroup("/articles")
            .RequireAuthorization()
            .WithTags("Articles");

        group.MapGet("/{articleGuid:guid}", GetSingleArticle)
            .WithName("GetArticle")
            .WithSummary("Gets a single article")
            .WithDescription("Retrieves a single article by its unique identifier. Optionally allows querying historical data using temporal tables.")
            .Produces<ArticleDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/", GetManyArticle)
            .WithName("GetArticles")
            .WithSummary("Gets multiple articles")
            .WithDescription("Retrieves a paginated list of articles. Supports optional temporal queries.")
            .Produces<IReadOnlyCollection<ArticleDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent);

        group.MapPost("/", CreateArticle)
            .WithName("CreateArticle")
            .WithSummary("Creates a new article")
            .WithDescription("Creates a new article and returns the generated identifier.")
            .Produces<Guid>(StatusCodes.Status200OK);

        group.MapPatch("/{articleGuid:guid}", UpdateArticle)
            .WithName("UpdateArticle")
            .WithSummary("Updates an existing article")
            .WithDescription("Updates an article using partial data.")
            .Produces(StatusCodes.Status204NoContent);

        group.MapDelete("/{articleGuid:guid}", DeleteArticle)
            .WithName("DeleteArticle")
            .WithSummary("Deletes an article")
            .WithDescription("Deletes the specified article from the system.")
            .Produces(StatusCodes.Status204NoContent);

        group.MapGet("/{articleGuid:guid}/partitions", GetArticlePartitions)
            .WithName("GetArticlePartitions")
            .WithSummary("Gets the partitions containing an article")
            .WithDescription("Returns the partitions that directly contain the specified article.")
            .Produces<IReadOnlyCollection<PartitionDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{articleGuid:guid}/partitions/{partitionGuid:guid}", AddArticlePartition)
            .WithName("AddArticlePartition")
            .WithSummary("Adds a partition to an article")
            .WithDescription("Associates an existing partition with the specified article.")
            .Produces(StatusCodes.Status204NoContent);

        group.MapDelete("/{articleGuid:guid}/partitions/{partitionGuid:guid}", RemoveArticlePartition)
            .WithName("RemoveArticlePartition")
            .WithSummary("Removes a partition from an article")
            .WithDescription("Removes the association between a partition and the specified article.")
            .Produces(StatusCodes.Status204NoContent);

        group.MapPut("/{articleGuid:guid}/image", UploadArticleImage)
            .WithName("UploadArticleImage")
            .WithSummary("Uploads or replaces the article image")
            .WithDescription("Stores an image for the article. If an image already exists it is replaced. Accepts multipart/form-data with a single file field named 'file'.")
            .Accepts<IFormFile>("multipart/form-data")
            .Produces(StatusCodes.Status204NoContent)
            .DisableAntiforgery();

        group.MapDelete("/{articleGuid:guid}/image", DeleteArticleImage)
            .WithName("DeleteArticleImage")
            .WithSummary("Removes the article image")
            .WithDescription("Deletes the image associated with the article. If no image exists the operation is a no-op.")
            .Produces(StatusCodes.Status204NoContent);

        group.MapGet("/{articleGuid:guid}/image", GetArticleImage)
            .WithName("GetArticleImage")
            .WithSummary("Streams the article image")
            .WithDescription("Returns the raw image bytes with the appropriate content type.")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{articleGuid:guid}/barcodes", GetArticleBarcodes)
            .WithName("GetArticleBarcodes")
            .WithSummary("Gets the barcodes of an article")
            .WithDescription("Returns all barcodes associated with the specified article.")
            .Produces<ArticleBarcodesDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPut("/{articleGuid:guid}/barcodes", UpdateArticleBarcodes)
            .WithName("UpdateArticleBarcodes")
            .WithSummary("Updates the barcodes of an article")
            .WithDescription("Replaces all barcodes associated with the specified article.")
            .Produces(StatusCodes.Status204NoContent);
    }

    private static async Task<Results<Ok<ArticleDto>, NotFound>> GetSingleArticle(
        Guid articleGuid,
        DateTimeOffset? temporalAsOf,
        IQueryHandler<ArticleSingleQuery, ArticleInformation?> handler,
        CancellationToken cancellationToken)
    {
        var query = new ArticleSingleQuery(articleGuid, temporalAsOf);

        var response = await handler.Handle(query, cancellationToken);

        return response is null ? TypedResults.NotFound() : TypedResults.Ok(response.ToContract());
    }

    private static async Task<Results<Ok<IReadOnlyCollection<ArticleDto>>, NoContent>> GetManyArticle(
        DateTimeOffset? temporalAsOf,
        Page? page,
        Limit? limit,
        Guid? partitionGuid,
        string? search,
        bool? noPartition,
        IQueryHandler<ArticleManyQuery, IReadOnlyCollection<ArticleInformation>> handler,
        CancellationToken cancellationToken)
    {
        var query = new ArticleManyQuery(
            temporalAsOf,
            PaginationHelpers.CreatePagination(page, limit),
            partitionGuid,
            search,
            noPartition
        );

        var response = await handler.Handle(query, cancellationToken);

        if (response.Count == 0)
        {
            return TypedResults.NoContent();
        }

        return TypedResults.Ok<IReadOnlyCollection<ArticleDto>>(response.Select(x => x.ToContract()).ToArray());
    }

    private static async Task<Ok<Guid>> CreateArticle(
        ArticleCreateRequest request,
        ICommandHandler<ArticleCreateCommand, Guid> handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.Handle(request.ToCommand(), cancellationToken);

        return TypedResults.Ok(response);
    }

    private static async Task<NoContent> UpdateArticle(
        Guid articleGuid,
        ArticleUpdateRequest request,
        ICommandHandler<ArticleUpdateCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(request.ToCommand(articleGuid), cancellationToken);

        return TypedResults.NoContent();
    }

    private static async Task<NoContent> DeleteArticle(
        Guid articleGuid,
        ICommandHandler<ArticleDeleteCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new ArticleDeleteCommand(articleGuid);

        await handler.Handle(command, cancellationToken);

        return TypedResults.NoContent();
    }

    private static async Task<Results<Ok<IReadOnlyCollection<PartitionDto>>, NotFound, NoContent>> GetArticlePartitions(
        Guid articleGuid,
        IQueryHandler<ArticlePartitionsQuery, IReadOnlyCollection<PartitionInformation>?> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new ArticlePartitionsQuery(articleGuid), cancellationToken);

        if (result is null)
        {
            return TypedResults.NotFound();
        }

        if (result.Count == 0)
        {
            return TypedResults.NoContent();
        }

        return TypedResults.Ok<IReadOnlyCollection<PartitionDto>>(result.Select(x => x.ToContract()).ToArray());
    }

    private static async Task<NoContent> UploadArticleImage(
        Guid articleGuid,
        IFormFile file,
        ICommandHandler<ArticleImageUploadCommand> handler,
        CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();

        var command = new ArticleImageUploadCommand(articleGuid, stream, file.ContentType);

        await handler.Handle(command, cancellationToken);

        return TypedResults.NoContent();
    }

    private static async Task<NoContent> DeleteArticleImage(
        Guid articleGuid,
        ICommandHandler<ArticleImageDeleteCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new ArticleImageDeleteCommand(articleGuid);

        await handler.Handle(command, cancellationToken);

        return TypedResults.NoContent();
    }

    private static async Task<Results<FileStreamHttpResult, NotFound>> GetArticleImage(
        Guid articleGuid,
        IQueryHandler<ArticleImageQuery, ArticleImageResult?> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new ArticleImageQuery(articleGuid), cancellationToken);

        if (result is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Stream(result.Stream, result.ContentType);
    }

    private static async Task<Results<Ok<ArticleBarcodesDto>, NotFound>> GetArticleBarcodes(
        Guid articleGuid,
        IQueryHandler<ArticleBarcodesQuery, Fargo.Domain.Articles.ArticleBarcodes?> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new ArticleBarcodesQuery(articleGuid), cancellationToken);

        if (result is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(result.ToContract());
    }

    private static async Task<NoContent> UpdateArticleBarcodes(
        Guid articleGuid,
        ArticleBarcodesDto barcodes,
        ICommandHandler<ArticleUpdateBarcodesCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(barcodes.ToCommand(articleGuid), cancellationToken);

        return TypedResults.NoContent();
    }

    private static async Task<NoContent> AddArticlePartition(
        Guid articleGuid,
        Guid partitionGuid,
        ICommandHandler<ArticleAddPartitionCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(new ArticleAddPartitionCommand(articleGuid, partitionGuid), cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> RemoveArticlePartition(
        Guid articleGuid,
        Guid partitionGuid,
        ICommandHandler<ArticleRemovePartitionCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(new ArticleRemovePartitionCommand(articleGuid, partitionGuid), cancellationToken);
        return TypedResults.NoContent();
    }
}
