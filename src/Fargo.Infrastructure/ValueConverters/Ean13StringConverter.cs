using Fargo.Core.Shared.Barcodes;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Fargo.Infrastructure.ValueConverters;

public class Ean13StringConverter()
    : ValueConverter<Ean13, string>(x => x.Value, x => new Ean13(x));
