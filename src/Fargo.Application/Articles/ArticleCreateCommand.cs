using Fargo.Application.Shared.Articles;
using Fargo.Core.Shared;
using Fargo.Core.Shared.Articles;
using Fargo.Core.Shared.Barcodes;
using System.Drawing;
using UnitsNet;

namespace Fargo.Application.Articles;

public sealed class ArticleCreateCommand : ICommand<Guid>
{
    public ArticleType ArticleType { get; private init; }

    public Name Name { get; init; }

    public Description? Description { get; init; } = null;

    public Guid? FromArticle { get; init; } = null;

    public Scalar? PackQuantity { get; init; } = null;

    public TimeSpan? ShelfLife { get; init; } = null;

    public Color? Color { get; init; } = null;

    public Mass? Mass { get; init; } = null;

    public Length? LengthX { get; init; } = null;

    public Length? LengthY { get; init; } = null;

    public Length? LengthZ { get; init; } = null;

    public Ean13? Ean13 { get; init; } = null;

    public Ean8? Ean8 { get; init; } = null;

    public UpcA? UpcA { get; init; } = null;

    public UpcE? UpcE { get; init; } = null;

    public Code128? Code128 { get; init; } = null;

    public Code39? Code39 { get; init; } = null;

    public Itf14? Itf14 { get; init; } = null;

    public Gs1128? Gs1128 { get; init; } = null;

    public QrCode? QrCode { get; init; } = null;

    public DataMatrix? DataMatrix { get; init; } = null;

    public IReadOnlyCollection<Guid>? PartitionsToAdd { get; init; } = null;

    public bool? IsActive { get; init; } = null;

    public ArticleCreateCommand(ArticleCreateDto dto)
    {
        ArticleType = dto.ArticleType ?? ArticleType.Default;

        if (ArticleType is ArticleType.Variation || ArticleType is ArticleType.Pack || ArticleType is ArticleType.Kit)
        {
            if (dto.FromArticle is null)
            {
                throw new ArgumentException(
                    "From article should be informed when article type is pack, variation or kit.", nameof(dto));
            }

            FromArticle = dto.FromArticle;
        }

        if (ArticleType is ArticleType.Pack)
        {
            if (dto.PackQuantity is null)
            {
                throw new ArgumentException(
                    "Pack quantity should be informed when article type is pack.", nameof(dto));
            }

            PackQuantity = dto.PackQuantity;
        }

        if (ArticleType is not ArticleType.Default && ArticleType is not ArticleType.Variation
            && ArticleType is not ArticleType.Pack && ArticleType is not ArticleType.Kit && ArticleType is not ArticleType.Container)
        {
            throw new ArgumentException("Article type not supported.", nameof(dto));
        }

        ShelfLife = dto.ShelfLife;

        Color = dto.Color;

        Mass = dto.Mass;

        LengthX = dto.LengthX;

        LengthY = dto.LengthY;

        LengthZ = dto.LengthZ;

        Ean13 = dto.Ean13;

        Ean8 = dto.Ean8;

        UpcA = dto.UpcA;

        UpcE = dto.UpcE;

        Code39 = dto.Code39;

        Itf14 = dto.Itf14;

        Gs1128 = dto.Gs1128;

        QrCode = dto.QrCode;

        DataMatrix = dto.DataMatrix;

        PartitionsToAdd = dto.PartitionsToAdd;
    }
}
