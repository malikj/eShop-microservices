//using Orders.Domain.Entities;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Orders.Domain.Entities;

//using Orders.Application.Ordering.Dtos;

//namespace Orders.Application.Abstractions.Repositories;

////public interface IOrderRepository
////{
////    Task AddAsync(Order order, CancellationToken cancellationToken = default);
////    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
////    Task UpdateAsync(Order order, CancellationToken cancellationToken);
////}

using Orders.Domain.Entities;

namespace Orders.Application.Abstractions.Repositories;

public interface IOrderRepository
{
    Task AddAsync(global::Orders.Domain.Entities.Order order, CancellationToken cancellationToken = default);
    Task<global::Orders.Domain.Entities.Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateAsync(global::Orders.Domain.Entities.Order order, CancellationToken cancellationToken);
}


//public interface IOrderRepository
//{
//    Task AddAsync(Order order, CancellationToken cancellationToken = default);
//    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
//    Task UpdateAsync(Order order, CancellationToken cancellationToken);
//}


//using Orders.Domain.Entities;

//namespace Orders.Application.Abstractions.Repositories;

//public interface IOrderRepository
//{
//    Task AddAsync(Order order, CancellationToken cancellationToken = default);
//    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
//    Task UpdateAsync(Order order, CancellationToken cancellationToken = default);
//}

