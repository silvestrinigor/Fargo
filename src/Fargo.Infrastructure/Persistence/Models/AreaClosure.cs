namespace Fargo.Infrastructure.Persistence.Models
{
    public class AreaClosure
    {
        public required Guid? AncestorGuid { get; set; }
        public required Guid? DescendantGuid { get; set; }
        public required int Depth { get; set; }
    }
}