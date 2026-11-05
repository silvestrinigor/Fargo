using Fargo.Core.Informations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Fargo.Infrastructure.Persistence.ValueConverters;

public class NameStringConverter()
    : ValueConverter<Name, string>(x => x.Value, x => new Name(x));
