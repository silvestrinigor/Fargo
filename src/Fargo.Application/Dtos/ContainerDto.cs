namespace Fargo.Application.Dtos;

public class ContainerDto : EntityDto
{
    public IEnumerable<Guid> ChildEntities { get; set; } = [];
}
