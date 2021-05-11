using MassTransit;
using Messaging.InterfacesConstants.Commands;
using Messaging.InterfacesConstants.Events;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Orders.API.Hubs;
using Orders.API.Models;
using Orders.API.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Orders.API.Messages.Consumers
{
    public class RegisterOrderCommandConsumer : IConsumer<IRegisterOrderCommand>
    {

        private readonly IOrderRepository _orderRepo;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IHubContext<OrderHub> _hubContext;
        private readonly IOptions<OrderSettings> _settings;
        private readonly ILogger<RegisterOrderCommandConsumer> _logger;

        public RegisterOrderCommandConsumer(IOrderRepository orderRepo, IHttpClientFactory clientFactory, IHubContext<OrderHub> hubContext, IOptions<OrderSettings> settings, ILogger<RegisterOrderCommandConsumer> logger)
        {
            _orderRepo = orderRepo;
            _clientFactory = clientFactory;
            _hubContext = hubContext;
            _settings = settings;
            _logger = logger;
        }
        public async Task Consume(ConsumeContext<IRegisterOrderCommand> context)
        {
            var result = context.Message;
            if (result?.OrderId != null && result.PictureUrl != null
                && result.UserEmail != null && result.ImageData != null)
            {
                if (await SaveOrderAsync(result))
                {
                    await _hubContext.Clients.All.SendAsync("UpdateOrders", "Order Created", result.OrderId);

                    var client = _clientFactory.CreateClient();
                    Tuple<List<byte[]>, Guid> orderDetailData = await GetFacesFromFaceApiAsync(client, result.ImageData, result.OrderId);
                    List<byte[]> faces = orderDetailData.Item1;
                    Guid orderId = orderDetailData.Item2;

                    _logger.LogInformation("Faces recieved from Faces.API");

                    bool status = await SaveOrderDetailsAsync(orderId, faces);
                    if (status)
                    {
                        await _hubContext.Clients.All.SendAsync("UpdateOrders", "Order Processed", orderId);
                        await context.Publish<IOrderProcessedEvent>(new
                        {
                            OrderId = orderId,
                            result.UserEmail,
                            Faces = faces,
                            result.PictureUrl
                        });
                        _logger.LogInformation("Order Processed Event Published.");
                    }
                }
            }
        }

        private async Task<Tuple<List<byte[]>, Guid>> GetFacesFromFaceApiAsync(HttpClient client, byte[] imageData, Guid orderId)
        {
            var byteContent = new ByteArrayContent(imageData);
            Tuple<List<byte[]>, Guid> orderDetailData = null;
            var url = _settings.Value.FacesApiUrl;
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            //using (var response = await client.PostAsync(_settings.Value.FacesApiUrl + "/api/facesazure?orderId=" + orderId, byteContent))
            using (var response = await client.PostAsync(_settings.Value.FacesApiUrl + "/api/faces?orderId=" + orderId, byteContent))
            {
                string apiResponse = await response.Content.ReadAsStringAsync();
                orderDetailData = JsonConvert.DeserializeObject<Tuple<List<byte[]>, Guid>>(apiResponse);
            }
            return orderDetailData;
        }

        private async Task<bool> SaveOrderDetailsAsync(Guid orderId, List<byte[]> faces)
        {
            var order = await _orderRepo.GetOrderAsync(orderId);
            if (order != null)
            {
                order.Status = Status.Processed;
                foreach (var face in faces)
                {
                    var orderDetail = new OrderDetail
                    {
                        OrderId = orderId,
                        FaceData = face
                    };
                    order.OrderDetails.Add(orderDetail);

                }
                bool status = await _orderRepo.UpdateOrderAsync(order);
                _logger.LogInformation("Order has been saved", status);

                return status;
            }
            else
            {
                _logger.LogError("Failed to save order", false);
                return false;
            }
        }

        private async Task<bool> SaveOrderAsync(IRegisterOrderCommand result)
        {
            Order order = new Order
            {
                OrderId = result.OrderId,
                UserEmail = result.UserEmail,
                Status = Status.Registered,
                PictureUrl = result.PictureUrl,
                ImageData = result.ImageData
            };
            return await _orderRepo.RegisterOrderAsync(order);
        }
    }
}
