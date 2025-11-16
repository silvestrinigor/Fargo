using Fargo.Core.Contracts;
using Fargo.Core.Entities;
using Fargo.Core.Entities.Abstracts;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence.Repositories
{
    public class SystemSettingRepository(FargoContext fagoContext) : ISystemSettingRepository
    {
        private readonly FargoContext fargoContext = fagoContext;

        public async Task<SystemSetting?> GetAsync(Guid guid)
            => await fargoContext.SystemSettings.Where(x => x.Guid == guid).FirstOrDefaultAsync();

        public async Task<IEnumerable<SystemSetting>> GetAsync()
            => await fargoContext.SystemSettings.ToListAsync();

        public async Task<IEnumerable<Guid>> GetGuidsAsync()
            => await fargoContext.SystemSettings.Select(x => x.Guid).ToListAsync();

        public void Add(SystemSetting sytemSetting)
            => fargoContext.SystemSettings.Add(sytemSetting);

        public void Remove(SystemSetting systemSetting)
            => fargoContext.SystemSettings.Remove(systemSetting);
    }
}
