using Faces.WebApp.ViewModels;
using Refit;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Faces.WebApp.RestClients
{
    public interface IOrderManagementApi
    {
        [Get("/orders")]
        Task<HttpResponseMessage> GetOrders();

        [Get("/orders/{orderId}")]
        Task<HttpResponseMessage> GetOrderById(Guid orderId);
    }
}
