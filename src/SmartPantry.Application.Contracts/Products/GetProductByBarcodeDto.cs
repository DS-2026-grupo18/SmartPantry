using System.ComponentModel.DataAnnotations;

namespace SmartPantry.Products;

public class GetProductByBarcodeDto
{
    [Required(ErrorMessage = "El código de barras es requerido.")]
    [StringLength(14, MinimumLength = 8, ErrorMessage = "El código de barras debe tener entre 8 y 14 dígitos.")]
    [RegularExpression(@"^[0-9]+$", ErrorMessage = "El código de barras solo debe contener dígitos.")]
    public string Barcode { get; set; } = string.Empty;
}
