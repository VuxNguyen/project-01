using System;

namespace WarehouseManagement.Models
{
    public class Brand
    {
        public string Name { get; set; } = string.Empty;
        public string LogoPath { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
