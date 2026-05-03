using Fargo.Domain.Articles;
using Fargo.Domain.Barcodes;
using UnitsNet;
using UnitsNet.Units;

namespace Fargo.Application.Articles;

/// <summary>
/// Conversions between domain Article entities and Application-layer Article models.
/// </summary>
public static class ArticleMappings
{
    public static ArticleDto ToInformation(this Article a)
    {
        ArgumentNullException.ThrowIfNull(a);

        return new ArticleDto(
            a.Guid,
            a.Name.ToString(),
            a.Description.ToString(),
            a.Metrics.ToModel(),
            a.ShelfLife,
            a.Barcodes.ToModel(),
            a.Partitions.Select(p => p.Guid).ToArray(),
            a.IsActive,
            a.EditedByGuid);
    }

    public static ArticleMetricsModel? ToModel(this ArticleMetrics? metrics)
    {
        if (metrics is null)
        {
            return null;
        }

        if (metrics.Mass is null && metrics.LengthX is null && metrics.LengthY is null && metrics.LengthZ is null)
        {
            return null;
        }

        return new ArticleMetricsModel(
            metrics.Mass.ToModel(),
            metrics.LengthX.ToModel(),
            metrics.LengthY.ToModel(),
            metrics.LengthZ.ToModel(),
            metrics.Density.ToModel());
    }

    public static MassModel? ToModel(this Mass? mass)
        => mass is null ? null : new MassModel(mass.Value.Value, Mass.GetAbbreviation(mass.Value.Unit));

    public static LengthModel? ToModel(this Length? length)
        => length is null ? null : new LengthModel(length.Value.Value, Length.GetAbbreviation(length.Value.Unit));

    public static DensityModel? ToModel(this Density? density)
        => density is null ? null : new DensityModel(density.Value.Value, Density.GetAbbreviation(density.Value.Unit));

    public static ArticleBarcodesModel ToModel(this ArticleBarcodes barcodes)
    {
        ArgumentNullException.ThrowIfNull(barcodes);

        return new ArticleBarcodesModel(
            barcodes.Ean13?.Code,
            barcodes.Ean8?.Code,
            barcodes.UpcA?.Code,
            barcodes.UpcE?.Code,
            barcodes.Code128?.Code,
            barcodes.Code39?.Code,
            barcodes.Itf14?.Code,
            barcodes.Gs1128?.Code,
            barcodes.QrCode?.Code,
            barcodes.DataMatrix?.Code);
    }

    public static ArticleBarcodes ToDomain(this ArticleBarcodesModel? model)
    {
        if (model is null)
        {
            return new ArticleBarcodes();
        }

        return new ArticleBarcodes
        {
            Ean13 = model.Ean13 is null ? null : new Ean13(model.Ean13),
            Ean8 = model.Ean8 is null ? null : new Ean8(model.Ean8),
            UpcA = model.UpcA is null ? null : new UpcA(model.UpcA),
            UpcE = model.UpcE is null ? null : new UpcE(model.UpcE),
            Code128 = model.Code128 is null ? null : new Code128(model.Code128),
            Code39 = model.Code39 is null ? null : new Code39(model.Code39),
            Itf14 = model.Itf14 is null ? null : new Itf14(model.Itf14),
            Gs1128 = model.Gs1128 is null ? null : new Gs1128(model.Gs1128),
            QrCode = model.QrCode is null ? null : new QrCode(model.QrCode),
            DataMatrix = model.DataMatrix is null ? null : new DataMatrix(model.DataMatrix),
        };
    }

    public static Mass? ToUnitsNet(this MassModel? mass)
        => mass is null ? null : Mass.From(mass.Value, UnitParser.Default.Parse<MassUnit>(mass.Unit));

    public static Length? ToUnitsNet(this LengthModel? length)
        => length is null ? null : Length.From(length.Value, UnitParser.Default.Parse<LengthUnit>(length.Unit));
}
