using Fargo.Application.Exceptions;
using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;

namespace Fargo.Application.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IBarcodeRepository"/>
/// to simplify retrieval operations with validation.
/// </summary>
public static class BarcodeRepositoryExtensions
{
    extension(IBarcodeRepository repository)
    {
        /// <summary>
        /// Retrieves a <see cref="Barcode"/> by its GUID and ensures it exists.
        /// </summary>
        /// <param name="barcodeGuid">The unique identifier of the barcode.</param>
        /// <param name="cancellationToken">A token used to cancel the operation.</param>
        /// <returns>The <see cref="Barcode"/> associated with the specified GUID.</returns>
        /// <exception cref="BarcodeNotFoundFargoApplicationException">
        /// Thrown when no barcode is found with the specified GUID.
        /// </exception>
        public async Task<Barcode> GetFoundByGuid(
            Guid barcodeGuid,
            CancellationToken cancellationToken = default
        )
        {
            var barcode = await repository.GetByGuid(barcodeGuid, cancellationToken)
                ?? throw new BarcodeNotFoundFargoApplicationException(barcodeGuid);

            return barcode;
        }
    }
}
