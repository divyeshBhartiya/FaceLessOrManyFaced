using Faces.WebApp.ViewModels;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Refit;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Faces.WebApp.RestClients
{
    public class OrderManagementApi : IOrderManagementApi
    {

        private IOrderManagementApi _restClient;
        private readonly IOptions<AppSettings> _settings;
        public OrderManagementApi(/*IConfiguration config,*/ HttpClient httpClient, IOptions<AppSettings> settings)
        {
            _settings = settings;
            string apiHostAndPort = _settings.Value.OrdersApiUrl;
            //config.GetSection("ApiServiceLocations").
            //GetValue<string>("OrdersApiLocation");
            httpClient.BaseAddress = new Uri($"{apiHostAndPort}/api");
            _restClient = RestService.For<IOrderManagementApi>(httpClient);

        }
        public async Task<HttpResponseMessage> GetOrderById(Guid orderId)
        {
            try
            {
                HttpResponseMessage response = await _restClient.GetOrderById(orderId);
                if (response.IsSuccessStatusCode) return response;
                else return null;
            }
            catch (ApiException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound) return null;
                else throw;
            }
        }

        public async Task<HttpResponseMessage> GetOrders()
        {
            HttpResponseMessage response = await _restClient.GetOrders();
            if (response.IsSuccessStatusCode) return response;
            else return null;
        }
    }
}
