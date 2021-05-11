using System;
using System.Collections.Generic;

namespace Orders.API.Models
{
    public class Order
    {
        public Order()
        {
            OrderDetails = new List<OrderDetail>();
        }
        public Guid OrderId { get; set; }
        public string PictureUrl { get; set; }
        public byte[] ImageData { get; set; }
        public string UserEmail { get; set; }
        public Status Status { get; set; }
        public List<OrderDetail> OrderDetails { get; set; }
    }
}
