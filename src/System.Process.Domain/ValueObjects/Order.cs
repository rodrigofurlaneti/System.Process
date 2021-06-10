using System.Collections.Generic;

namespace System.Process.Domain.ValueObjects
{
    public class Order
    {
        public string MerchantId { get; set; }
        public string OrderNumber { get; set; }
        public string PosNumber { get; set; }
        public string Date { get; set; }
        public Address ShippingAddress { get; set; }
        public Address BillingAddress { get; set; }
        public IList<Product> Products { get; set; }
        public Term Equipment { get; set; }
    }
}
