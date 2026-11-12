using System;
using System.ComponentModel.DataAnnotations;
namespace SmartPantry.Pantry;

public class UpdatePantryItemDto
{
    [Range(0, 999999, ErrorMessage = "Pantry:QuantityCannotBeNegative")]
    public decimal? Quantity { get; set; }
    [StringLength(50, ErrorMessage = "Pantry:UnitTooLong")]
    public string? Unit { get; set; }
    public DateTime? ExpirationDate { get; set; }
}