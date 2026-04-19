using Fargo.Domain.Users;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Fargo.Infrastructure.Converters;

public class PasswordHashStringConverter()
    : ValueConverter<PasswordHash, string>(x => x.Value, x => new(x));
