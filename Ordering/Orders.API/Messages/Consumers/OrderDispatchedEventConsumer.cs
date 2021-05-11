using MassTransit;
using Messaging.InterfacesConstants.Events;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Orders.API.Hubs;
using Orders.API.Models;
using Orders.API.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orders.API.Messages.Consumers
{
    public class OrderDispatchedEventConsumer : IConsumer<IOrderDispatchedEvent>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IHubContext<OrderHub> _hubContext;
        private readonly ILogger<OrderDispatchedEventConsumer> _logger;
        public OrderDispatchedEventConsumer(IOrderRepository orderRepository, IHubContext<OrderHub> hubContext, ILogger<OrderDispatchedEventConsumer> logger)
        {
            _orderRepository = orderRepository;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<IOrderDispatchedEvent> context)
        {
            var message = context.Message;
            Guid orderId = message.OrderId;
            bool status = await UpdateDatabaseAsync(orderId);
            if (status)
            {
                _logger.LogInformation("Updated order details in the database", status);
            }
            else
            {
                _logger.LogError("Failed to update order details in the database", status);
            }
        }
        private async Task<bool> UpdateDatabaseAsync(Guid orderId)
        {
            var order = await _orderRepository.GetOrderAsync(orderId);
            if (order != null)
            {
                order.Status = Status.Sent;
                bool status = await _orderRepository.UpdateOrderAsync(order);
                if (status)
                {
                    await _hubContext.Clients.All.SendAsync("UpdateOrders", "Dispatched", orderId);
                }
                return status;
            }
            else
            {
                return false;
            }
        }
    }
}
