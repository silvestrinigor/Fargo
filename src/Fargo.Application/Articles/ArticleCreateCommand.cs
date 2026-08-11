using Fargo.Application.Common;
using Fargo.Application.Shared.Articles;
using Fargo.Core.Shared.Articles;

namespace Fargo.Application.Articles;

public sealed class ArticleCreateCommand : ICommand<Guid>
{
    public ArticleCreateDto Create { get; private init; }

    public ArticleCreateCommand(ArticleCreateDto dto)
    {
        ValidateDto(dto);

        Create = dto;
    }

    private static void ValidateDto(ArticleCreateDto dto)
    {
        if (dto.ArticleType is ArticleType.Variation)
        {
            if (dto.Variation?.FromArticleGuid is null)
            {
                throw new ArgumentException(
                    "Variation from article guid must be informed when the article type is variation.", nameof(dto));
            }
        }

        else if (dto.ArticleType is ArticleType.Pack)
        {
            if (dto.Pack?.FromArticleGuid is null)
            {
                throw new ArgumentException(
                    "Pack from article guid must be informed when the article type is pack.", nameof(dto));
            }

            if (dto.Pack?.Quantity is null)
            {
                throw new ArgumentException(
                    "Pack quantity should be informed when article type is pack.", nameof(dto));
            }
        }

        else if (dto.ArticleType is ArticleType.Kit)
        {
            if (dto.KitComponents is null || dto.KitComponents.Count == 0)
            {
                throw new ArgumentException(
                    "Kit components should be informed when article type is kit.", nameof(dto));
            }
        }

        else if (
            dto.ArticleType != ArticleType.Default &&
            dto.ArticleType != ArticleType.Container &&
            dto.ArticleType != null)
        {
            throw new ArgumentException("Article type not supported.", nameof(dto));
        }
    }
}
