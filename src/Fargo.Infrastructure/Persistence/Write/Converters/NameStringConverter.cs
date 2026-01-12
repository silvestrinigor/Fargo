using Fargo.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Fargo.Infrastructure.Persistence.Write.Converters
{
    public class NameStringConverter()
        : ValueConverter<Name, string>(x => x.Value, x => new Name(x));
}
