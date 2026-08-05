using Fargo.Core.Shared.Informations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Fargo.Infrastructure.ValueConverters;

public class LastNameStringConverter()
    : ValueConverter<LastName, string>(x => x.Value, x => new LastName(x));
