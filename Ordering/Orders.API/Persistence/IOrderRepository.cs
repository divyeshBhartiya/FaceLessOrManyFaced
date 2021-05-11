using Orders.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orders.API.Persistence
{
    public interface IOrderRepository
    {
        Task<Order> GetOrderAsync(Guid id);
        Task<IEnumerable<Order>> GetOrdersAsync();
        Task<bool> RegisterOrderAsync(Order order);
        Order GetOrder(Guid id);
        Task<bool> UpdateOrderAsync(Order order);
    }
}
