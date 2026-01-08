namespace Fargo.Application.Dtos
{
    public sealed record PaginationDto(
        int? Page,
        int? Limit
        )
    {
        public int? Skip => (Page - 1) * Limit;
    }
}
