using Fargo.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Fargo.Infrastructure.Persistence.Converters
{
    public class DescriptionStringConverter()
        : ValueConverter<Description, string>(x => x.Value, x => new Description(x));
}