using Fargo.Domain.Users;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Fargo.Infrastructure.Converters;

public class FirstNameStringConverter()
    : ValueConverter<FirstName, string>(x => x.Value, x => new FirstName(x));
