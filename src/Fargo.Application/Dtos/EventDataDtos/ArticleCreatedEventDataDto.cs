using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Dtos.EventDataDtos
{
    public sealed record ArticleCreatedEventDataDto(
        Name Name
        );
}
