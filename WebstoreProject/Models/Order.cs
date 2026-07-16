using System;
using System.Collections.Generic;

namespace WebstoreProject.Models
{
    public class Order
    {
        public int OrderId { get; set; }

        public string UserId { get; set; } = "";

        public DateTime OrderDate { get; set; }

        public string Status { get; set; } = "New";

        public int DeliveryTypeId { get; set; }
        public DeliveryType? DeliveryType { get; set; }

        public decimal DeliveryFee { get; set; }
        public decimal TotalPrice { get; set; }

        public string DeliveryAddress { get; set; } = "";
        public string PaymentMethod { get; set; } = "Cash";

        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
