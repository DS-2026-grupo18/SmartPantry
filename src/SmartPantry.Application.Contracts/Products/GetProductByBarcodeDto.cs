using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SmartPantry.Products
{
    public class GetProductByBarcodeDto
    {
        [Required]
        [StringLength(14, MinimumLength = 8)]
        public string Barcode { get; set; } = string.Empty;
    }
}
