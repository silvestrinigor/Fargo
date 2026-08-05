using Fargo.Core.Shared.Informations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Fargo.Infrastructure.ValueConverters;

public class NameStringConverter()
    : ValueConverter<Name, string>(x => x.Value, x => new Name(x));
