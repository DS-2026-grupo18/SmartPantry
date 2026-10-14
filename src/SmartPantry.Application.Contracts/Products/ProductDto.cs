using System;
using Volo.Abp.Application.Dtos;

namespace SmartPantry.Products;

public class ProductDto : AuditedEntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;

    public string Brand { get; set; } = string.Empty;
}
