using System;
using System.Collections.Generic;
using System.Text;

namespace SmartPantry.Products
{
    public class ExternalProductDto
    {
        public string Barcode { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? Brand { get; set; }
        public string? ImageUrl { get; set; }
        public string? Ingredients { get; set; }
        public string? NutriScore { get; set; }
    }
}
