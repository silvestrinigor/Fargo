using Fargo.Application.Shared.Articles;
using Fargo.Core.Shared.Articles;

namespace Fargo.Application.Articles;

public sealed class ArticleCreateCommand : ICommand<Guid>
{
    public ArticleCreateDto Create { get; private init; }

    public ArticleCreateCommand(ArticleCreateDto dto)
    {
        Create = dto;

        if (dto.ArticleType is ArticleType.Variation
            || dto.ArticleType is ArticleType.Pack
            || dto.ArticleType is ArticleType.Kit)
        {
            if (dto.Variation?.FromArticleGuid is null)
            {
                throw new ArgumentException(
                    "From article should be informed when article type is pack, variation or kit.", nameof(dto));
            }
        }

        if (dto.ArticleType is ArticleType.Pack)
        {
            if (dto.Pack?.Quantity is null)
            {
                throw new ArgumentException(
                    "Pack quantity should be informed when article type is pack.", nameof(dto));
            }
        }

        if (dto.ArticleType is ArticleType.Kit)
        {
            if (dto.KitComponents is null || dto.KitComponents.Count == 0)
            {
                throw new ArgumentException(
                    "Kit components should be informed when article type is kit.", nameof(dto));
            }
        }

        if (dto.ArticleType is not ArticleType.Default && dto.ArticleType is not ArticleType.Variation
            && dto.ArticleType is not ArticleType.Pack && dto.ArticleType is not ArticleType.Kit
            && dto.ArticleType is not ArticleType.Container)
        {
            throw new ArgumentException("Article type not supported.", nameof(dto));
        }
    }
}
