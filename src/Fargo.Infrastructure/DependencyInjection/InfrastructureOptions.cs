using Fargo.Application.Identity;
using Fargo.Infrastructure.Security;
using Microsoft.Extensions.DependencyInjection;

namespace Fargo.Infrastructure;

public sealed class InfrastructureOptions
{
    internal Action<IServiceCollection>? ConfigureCurrentUser { get; private set; }

    public void UseHttpCurrentActor()
    {
        ConfigureCurrentUser = services =>
        {
            services.AddScoped<ICurrentActor, CurrentUserActorHttp>();
        };
    }

    public void UseSystemCurrentActor()
    {
        ConfigureCurrentUser = services =>
        {
            services.AddSingleton<ICurrentActor, CurrentActorSystem>();
        };
    }

    public void UserEmptyCurrentUser()
    {
        ConfigureCurrentUser = services =>
        {
            services.AddSingleton<ICurrentActor, CurrentActorEmpty>();
        };
    }
}