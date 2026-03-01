using Fargo.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Fargo.Infrastructure.Persistence.Converters
{
    public class PasswordHashStringConverter()
        : ValueConverter<PasswordHash, string>(x => x.Value, x => new(x));
}