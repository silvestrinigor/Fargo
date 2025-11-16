using System;
using System.Collections.Generic;
using System.Text;

namespace Fargo.Application.Contracts.Http
{
    public interface IRootEntitiesService
    {
        Task EnsureRootAreaExistAsync();
    }
}
