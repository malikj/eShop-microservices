using Catalog.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalog.Domain.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<Product>> GetByIdsAsync(IEnumerable<Guid> ids);

    Task<IEnumerable<Product>> GetAllAsync();
    Task AddAsync(Product product);
}


