using Fargo.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Fargo.Infrastructure.Converters
{
    public class NameStringConverter()
        : ValueConverter<Name, string>(x => x.ToString(), x => Name.NewName(x));
}
