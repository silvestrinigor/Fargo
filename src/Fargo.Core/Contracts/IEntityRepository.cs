using Fargo.Core.Entities.Abstracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fargo.Core.Contracts;

public interface IEntityRepository<TEntity> where TEntity : Entity
{
    Task<TEntity?> GetAsync(Guid guid);
    Task<IEnumerable<TEntity>> GetAsync();
    void Add(TEntity container);
    void Remove(TEntity container);
}
