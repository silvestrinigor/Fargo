using Fargo.Application.Contracts.Http;
using Fargo.Application.Contracts.Persistence;
using Fargo.Core.Contracts;
using Fargo.Core.Entities;
using Fargo.Core.Enums;
using Fargo.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fargo.Application.Services
{
    public class RootEntitiesService(IAreaRepository areaRepository, IUnitOfWork unitOfWork) : IRootEntitiesService
    {
        private readonly IAreaRepository areaRepository = areaRepository;
        private readonly IUnitOfWork unitOfWork = unitOfWork;

        public async Task EnsureRootAreaExistAsync()
        {
            if (!await areaRepository.AnyAsync())
            {
                var root = new Area { Name = "Root Area", Guid = AreaService.AreaRootGuid, EntityType = EEntityType.Area, IsGlobalArea = true };
                areaRepository.Add(root);
                await unitOfWork.SaveChangesAsync();
            }    
        }
    }
}
