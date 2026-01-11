using Fargo.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Fargo.Infrastructure.Persistence.Write.Converters
{
    public class NameStringConverter()
        : ValueConverter<Name, string>(x => x.ToString(), x => new Name(x));
}
