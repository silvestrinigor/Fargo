using Fargo.Application.Persistence;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Fargo.Infrastructure.DependencyInjection;

public static class ApiServiceExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddInfrastructure()
        {
            services.AddDbContext<FargoContext>(opt =>
                opt.UseInMemoryDatabase("Fargo"));

            services.AddScoped<IUnitOfWork, FargoUnitOfWork>();

            return services;
        }
    }

    extension(IServiceProvider services)
    {
        public async Task<IServiceProvider> InitInfrastructureAsync()
        {
            using (var scope = services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<FargoContext>();
                dbContext.Database.EnsureCreated();
            }

            return services;
        }
    }
}
