using Fargo.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Fargo.Infrastructure.Converters
{
    public class DescriptionStringConverter()
        : ValueConverter<Description, string>(x => x.ToString(), x => new Description(x));
}
