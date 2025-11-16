using Fargo.Core.Contracts;
using Fargo.Core.Entities;
using Fargo.Core.Constants;
using Fargo.Core.Entities.Abstracts;

namespace Fargo.Core.Services
{
    public class AreaService(IAreaRepository areaRepository)
    {
        public static Guid AreaRootGuid { get; } = Guid.Parse(EntitiesConstants.AreaRootGuid);

        private readonly IAreaRepository areaRepository = areaRepository;

        public async Task AddEntityIntoAreaAsync(Area area, Entity entity)
        {
            await areaRepository.AddEntityIntoAreaAsync(area, entity);
        }

        public async Task RemoveEntityFromAreaAsync(Area area, Entity entity)
        {
            await areaRepository.RemoveEntityFromAreaAsync(area, entity);
        }
    }
}