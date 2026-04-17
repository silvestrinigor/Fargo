using Fargo.Domain;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Fargo.Infrastructure.Converters;

public class DescriptionStringConverter()
    : ValueConverter<Description, string>(x => x.Value, x => new Description(x));
