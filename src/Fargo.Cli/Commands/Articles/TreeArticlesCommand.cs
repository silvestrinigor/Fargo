using Fargo.Cli.Output;
using Fargo.Client;
using Fargo.Client.Models;
using System.CommandLine;

namespace Fargo.Cli.Commands.Articles;

/// <summary>
/// Renders articles as a tree, nesting each variation and pack under the article it derives from.
/// </summary>
/// <remarks>
/// A variation nests under <see cref="ArticleVariationDto.FromArticleGuid"/> and a pack under
/// <see cref="ArticlePackDto.FromArticleGuid"/>. Every other article, kits included, is a root,
/// as is a variation or pack whose source article is not visible.
/// </remarks>
public sealed class TreeArticlesCommand : Command
{
    // Mirrors the server's ArticleType enum numeric values.
    private const int ArticleTypeVariation = 2;
    private const int ArticleTypePack = 3;
    private const int ArticleTypeKit = 4;
    private const int ArticleTypeContainer = 5;

    public TreeArticlesCommand(FargoApiClient client)
        : base("tree", """
            Show articles as a tree: each variation and pack nested under the
            article it derives from.

            Each line is tagged with its type ([article], with a variation / pack
            / kit / container hint). Kits are not nested and always appear at the
            root level. Variations and packs whose source article is not visible
            also appear at the root level.

            Examples:
              fargo article tree
            """)
    {
        SetAction(async (_, cancellationToken) =>
        {
            var articles = (await client.V1.Articles.GetAsync(cancellationToken: cancellationToken))?.Items ?? [];

            var articlesByGuid = new Dictionary<Guid, ArticleDto>();

            foreach (var article in articles)
            {
                if (article.Guid is { } guid)
                {
                    articlesByGuid[guid] = article;
                }
            }

            var childrenByParent = new Dictionary<Guid, List<ArticleDto>>();
            var rootArticles = new List<ArticleDto>();

            foreach (var article in articles)
            {
                var parentGuid = GetParentArticleGuid(article);

                // Nest only when the source article is visible; otherwise the
                // article is shown at the root level.
                if (parentGuid is { } pg && articlesByGuid.ContainsKey(pg))
                {
                    if (!childrenByParent.TryGetValue(pg, out var children))
                    {
                        childrenByParent[pg] = children = [];
                    }

                    children.Add(article);
                }
                else
                {
                    rootArticles.Add(article);
                }
            }

            var roots = new List<TreeNode>();
            var visited = new HashSet<Guid>();

            foreach (var article in rootArticles.OrderBy(a => a.Name, StringComparer.OrdinalIgnoreCase))
            {
                roots.Add(BuildArticleNode(article, childrenByParent, visited));
            }

            if (roots.Count == 0)
            {
                Console.WriteLine("No articles found.");
                return 0;
            }

            TreeWriter.WriteForest(roots);
            return 0;
        });
    }

    private static Guid? GetParentArticleGuid(ArticleDto article)
    {
        return article.ArticleType switch
        {
            ArticleTypeVariation => article.Variation?.FromArticleGuid,
            ArticleTypePack => article.Pack?.FromArticleGuid,
            _ => null,
        };
    }

    private static TreeNode BuildArticleNode(
        ArticleDto article,
        IReadOnlyDictionary<Guid, List<ArticleDto>> childrenByParent,
        HashSet<Guid> visited)
    {
        var node = new TreeNode(ArticleLabel(article));

        if (article.Guid is not { } guid || !visited.Add(guid))
        {
            // Missing GUID or already visited (cycle) -> do not recurse.
            return node;
        }

        if (childrenByParent.TryGetValue(guid, out var children))
        {
            foreach (var child in children.OrderBy(a => a.Name, StringComparer.OrdinalIgnoreCase))
            {
                node.Add(BuildArticleNode(child, childrenByParent, visited));
            }
        }

        return node;
    }

    private static string ArticleLabel(ArticleDto article)
    {
        var name = article.Name ?? article.Guid?.ToString() ?? "(unknown)";
        var label = $"[article] {name}";

        var typeHint = article.ArticleType switch
        {
            ArticleTypeVariation => " [variation]",
            ArticleTypePack => " [pack]",
            ArticleTypeKit => " [kit]",
            ArticleTypeContainer => " [container]",
            _ => string.Empty,
        };

        return label + typeHint;
    }
}
