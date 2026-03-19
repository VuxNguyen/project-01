using System;

namespace WarehouseManagement.Models
{
    public class Helmet
    {
        public string Id { get; set; } = string.Empty;
        public string HelmetType { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public DateTime ImportDate { get; set; }
        public string Material { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Color { get; set; } = string.Empty;
        public double Weight { get; set; }
    }
}
