namespace Fargo.Application.Dtos;

public class EntityDto
{
    public Guid Guid { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public DateTime? CreatedAt { get; set; }
}