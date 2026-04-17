using Fargo.Domain.Entities;
using System.Collections.ObjectModel;

namespace Fargo.Domain.Collections;

/// <summary>
/// Represents a collection of <see cref="Barcode"/> instances associated with an article.
/// </summary>
/// <remarks>
/// Enforces the domain rule that an article may have at most one barcode per format.
/// </remarks>
public sealed class BarcodeCollection : Collection<Barcode>
{
    /// <summary>
    /// Initializes an empty <see cref="BarcodeCollection"/>.
    /// </summary>
    public BarcodeCollection()
    {
    }

    /// <summary>
    /// Initializes a new <see cref="BarcodeCollection"/> with the specified barcodes.
    /// </summary>
    /// <param name="barcodes">The barcodes to populate the collection with.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="barcodes"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the collection would contain duplicate formats.</exception>
    public BarcodeCollection(IEnumerable<Barcode> barcodes)
    {
        ArgumentNullException.ThrowIfNull(barcodes);

        foreach (var barcode in barcodes)
        {
            Add(barcode);
        }
    }

    /// <inheritdoc />
    protected override void InsertItem(int index, Barcode item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (Items.Any(b => b.Format == item.Format))
        {
            throw new InvalidOperationException(
                $"A barcode with format '{item.Format}' already exists in the collection.");
        }

        base.InsertItem(index, item);
    }

    /// <inheritdoc />
    protected override void SetItem(int index, Barcode item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (Items.Any(b => b.Format == item.Format) && !ReferenceEquals(Items[index], item))
        {
            throw new InvalidOperationException(
                $"A barcode with format '{item.Format}' already exists in the collection.");
        }

        base.SetItem(index, item);
    }
}
