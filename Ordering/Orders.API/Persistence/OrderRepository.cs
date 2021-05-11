using Microsoft.EntityFrameworkCore;
using Orders.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orders.API.Persistence
{
    public class OrderRepository : IOrderRepository
    {
        private OrdersContext _context;

        public OrderRepository(OrdersContext context)
        {
            _context = context;
        }
        public Order GetOrder(Guid id)
        {
            return _context.Orders.
                Include("OrderDetails").
                FirstOrDefault(c => c.OrderId == id);
        }

        public async Task<Order> GetOrderAsync(Guid id)
        {
            return await _context.Orders.
                Include("OrderDetails").
                FirstOrDefaultAsync(c => c.OrderId == id);
        }

        public async Task<IEnumerable<Order>> GetOrdersAsync()
        {
            return await _context.Orders.ToListAsync();
        }

        public async Task<bool> RegisterOrderAsync(Order order)
        {
            try
            {
                _context.Add(order);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateOrderAsync(Order order)
        {
            try
            {
                _context.Entry(order).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
