using Fargo.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Fargo.Infrastructure.Persistence.Converters
{
    public class NameidStringConverter()
        : ValueConverter<Nameid, string>(x => x.Value, x => new Nameid(x));
}