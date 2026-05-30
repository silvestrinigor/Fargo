using Fargo.Core.Shared;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Fargo.Infrastructure.Converters;

public class LastNameStringConverter()
    : ValueConverter<LastName, string>(x => x.Value, x => new LastName(x));
