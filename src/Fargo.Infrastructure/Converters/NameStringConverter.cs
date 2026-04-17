using Fargo.Domain;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Fargo.Infrastructure.Converters;

public class NameStringConverter()
    : ValueConverter<Name, string>(x => x.Value, x => new Name(x));
