using Fargo.Core.Informations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Fargo.Infrastructure.Persistence.ValueConverters;

public class LastNameStringConverter()
    : ValueConverter<LastName, string>(x => x.Value, x => new LastName(x));
