using Fargo.Domain.Abstracts.Entities;
using Fargo.Domain.Contracts;
using Fargo.Domain.Entities;
using Fargo.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace Fargo.Infrastructure.Persistence.Repositories
{
    public class AreaRepository(FargoContext fargoContext) : IAreaRepository
    {
        private readonly FargoContext fargoContext = fargoContext;

        public async Task<bool> AnyAsync() =>
            await fargoContext.Areas.AnyAsync();

        public async Task<Area?> GetAsync(Guid guid) =>
            await fargoContext.Areas.Where(x => x.Guid == guid).FirstOrDefaultAsync();

        public async Task<IEnumerable<Area>> GetAsync() =>
            await fargoContext.Areas.ToListAsync();

        public async Task<IEnumerable<Guid>> GetGuidsAsync() =>
            await fargoContext.Areas.Select(x => x.Guid).ToListAsync();

        public async Task RemoveAsync(Area area)
        {
            fargoContext.Areas.Remove(area);

            var parents = await fargoContext.AreaClosure
                .Where(x => x.DescendantGuid == area.Guid && x.Depth == 1)
                .Select(c => c.AncestorGuid)
                .ToListAsync();

            var children = await fargoContext.AreaClosure
                .Where(x => x.AncestorGuid == area.Guid && x.Depth == 1)
                .Select(c => c.DescendantGuid)
                .ToListAsync();

            if(!parents.Any())
            {
                throw new InvalidOperationException("Cannot remove area without parents.");
            }

            await fargoContext.AreaClosure
                .Where(x => x.AncestorGuid == area.Guid || x.DescendantGuid == area.Guid)
                .ExecuteDeleteAsync();

            foreach (var parent in parents)
            foreach (var child in children)
            {
                    var parentAncestors = await fargoContext.AreaClosure
                        .Where(c => c.DescendantGuid == parent)
                        .ToListAsync();

                    // All descendants of child
                    var childDescendants = await fargoContext.AreaClosure
                        .Where(c => c.AncestorGuid == child)
                        .ToListAsync();

                    var newEntries = new List<AreaClosure>();

                    foreach (var p in parentAncestors)
                        foreach (var c in childDescendants)
                        {
                            newEntries.Add(new AreaClosure
                            {
                                AncestorGuid = p.AncestorGuid,
                                DescendantGuid = c.DescendantGuid,
                                Depth = p.Depth + 1 + c.Depth
                            });
                        }

                    fargoContext.AreaClosure.AddRange(newEntries);
                }
        }

        public async Task AddAsync(Area area)
        {
            fargoContext.Areas.Add(area);

            fargoContext.AreaClosure.Add(new AreaClosure
            {
                AncestorGuid = area.Guid,
                DescendantGuid = area.Guid,
                Depth = 0
            });

            if (area.IsGlobalArea)
                return;

            var globalArea = await fargoContext.Areas.FirstAsync(x => x.IsGlobalArea);

            var ancestors = await fargoContext.AreaClosure
                .Where(x => x.DescendantGuid == globalArea.Guid)
                .ToListAsync();

            ancestors.ForEach(x =>
            {
                fargoContext.AreaClosure.Add(new AreaClosure
                {
                    AncestorGuid = x.AncestorGuid,
                    DescendantGuid = area.Guid,
                    Depth = x.Depth + 1
                });
            });
        }

        public async Task<IEnumerable<NamedEntity>> GetAreaEntitiesAsync(Guid areaGuid)
            => await fargoContext.AreaClosure
                .Where(x => x.AncestorGuid == areaGuid && x.Depth > 0)
                .Join(
                    fargoContext.Entities,
                    closure => closure.DescendantGuid,
                    entity => entity.Guid,
                    (closure, entity) => entity
                )
                .ToListAsync();

        public async Task AddEntityIntoAreaAsync(Area areaGuid, NamedEntity entityGuid)
        {
            throw new NotImplementedException();
        }

        public Task RemoveEntityFromAreaAsync(Area areaGuid, NamedEntity entityGuid)
        {
            throw new NotImplementedException();
        }
    }
}
