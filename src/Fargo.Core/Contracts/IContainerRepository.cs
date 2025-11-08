using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fargo.Core.Entities;

namespace Fargo.Core.Contracts;

public interface IContainerRepository : IEntityRepository<Container>
{
    Task<Container?> GetEntityContainer(Guid guid);
}
