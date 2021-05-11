using Faces.WebApp.RestClients;
using Faces.WebApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
            var response = await _ordersApi.GetOrders();
            var return_value = await response.Content.ReadAsStringAsync();
            List<OrderViewModel> Orders = JsonConvert.DeserializeObject<List<OrderViewModel>>(return_value);

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
            var response = await _ordersApi.GetOrderById(Guid.Parse(orderId));
            var return_value = await response.Content.ReadAsStringAsync();
            OrderViewModel order = JsonConvert.DeserializeObject<OrderViewModel>(return_value);
            order.ImageString = string.Format("data:image/png;base64,{0}", Convert.ToBase64String(order.ImageData));

            foreach (var detail in order.OrderDetails)
            {
                detail.ImageString = string.Format("data:image/png;base64,{0}", Convert.ToBase64String(detail.FaceData));
            }
            return View(order);
        }

    }
}
