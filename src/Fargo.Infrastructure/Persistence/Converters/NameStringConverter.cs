using Fargo.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Fargo.Infrastructure.Persistence.Converters
{
    public class NameStringConverter()
        : ValueConverter<Name, string>(x => x.Value, x => new Name(x));
}