using Faces.WebApp.RestClients;
using Faces.WebApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Faces.WebApp.Controllers
{
    [Route("OrderManagement")]
    public class OrderManagementController : Controller
    {
        private readonly IOrderManagementApi _ordersApi;

        public OrderManagementController(IOrderManagementApi ordersApi)
        {
            _ordersApi = ordersApi;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var Orders = await _ordersApi.GetOrders();
            var model = new OrderListViewModel
            {
                Orders = Orders
            };

            foreach (var m in model.Orders)
            {
                string imageBase64Data = Convert.ToBase64String(m.ImageData);
                m.ImageString = string.Format("data:image/png;base64,{0}", imageBase64Data);
            }
            return View(model);
        }

        [Route("/Details/{orderId}")]
        public async Task<IActionResult> Details(string orderId)
        {
            var order = await _ordersApi.GetOrderById(Guid.Parse(orderId));
            order.ImageString = string.Format("data:image/png;base64,{0}",
                Convert.ToBase64String(order.ImageData));
            foreach (var detail in order.OrderDetails)
            {
                detail.ImageString = string.Format("data:image/png;base64,{0}",
                Convert.ToBase64String(detail.FaceData));
            }
            return View(order);
        }

    }
}
