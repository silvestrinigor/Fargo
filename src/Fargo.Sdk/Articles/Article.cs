using Fargo.Sdk.Partitions;
using Microsoft.Extensions.Logging;

namespace Fargo.Sdk.Articles;

/// <summary>
/// Represents a live article entity. Setting <see cref="Name"/> or <see cref="Description"/>
/// automatically sends a PATCH request to the backend to persist the change.
/// </summary>
public sealed class Article
{
    internal Article(Guid guid, string name, string description, IArticleClient client, ILogger logger)
    {
        Guid = guid;
        _name = name;
        _description = description;
        this.client = client;
        this.logger = logger;
    }

    private readonly IArticleClient client;
    private readonly ILogger logger;

    /// <summary>The unique identifier of the article.</summary>
    public Guid Guid { get; }

    private string _name;

    /// <summary>
    /// The display name of the article. Setting this property fires a background update request.
    /// </summary>
    public string Name
    {
        get => _name;
        set
        {
            if (_name == value)
            {
                return;
            }

            _name = value;
            _ = SendUpdateAsync();
        }
    }

    private string _description;

    /// <summary>
    /// The description of the article. Setting this property fires a background update request.
    /// </summary>
    public string Description
    {
        get => _description;
        set
        {
            if (_description == value)
            {
                return;
            }

            _description = value;
            _ = SendUpdateAsync();
        }
    }

    /// <summary>
    /// Gets the partitions that directly contain this article.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    public Task<FargoSdkResponse<IReadOnlyCollection<PartitionResult>>> GetPartitionsAsync(
        CancellationToken cancellationToken = default)
        => client.GetPartitionsAsync(Guid, cancellationToken);

    private async Task SendUpdateAsync()
    {
        var result = await client.UpdateAsync(Guid, _name, _description);

        if (!result.IsSuccess)
        {
            logger.LogArticleUpdateFailed(Guid, result.Error!.Detail);
        }
    }
}
