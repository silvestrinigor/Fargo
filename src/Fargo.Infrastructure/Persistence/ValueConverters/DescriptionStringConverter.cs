using Fargo.Core.Informations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Fargo.Infrastructure.Persistence.ValueConverters;

public class DescriptionStringConverter()
    : ValueConverter<Description, string>(x => x.Value, x => new Description(x));
