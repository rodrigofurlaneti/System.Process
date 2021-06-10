using System.Collections.Generic;

namespace System.Process.Domain.ValueObjects
{
    public class Product
    {
        public string Description { get; set; }
        public string ProductId { get; set; }
        public string TpgId { get; set; }
        public string Manufacturer { get; set; }
        public int Quantity { get; set; }
        public decimal? Cost { get; set; }
        public string Model { get; set; }
        public string ChargeMode { get; set; }
        public IList<Equipment> Equipments { get; set; }
    }
}