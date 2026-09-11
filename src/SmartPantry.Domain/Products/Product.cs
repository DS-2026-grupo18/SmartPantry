using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SmartPantry.Products;

public class Product : AuditedAggregateRoot<Guid>
{
    public string Name { get; private set; } = null!;

    public string Brand { get; private set; } = null!;

    protected Product()
    {
        /* Constructor protegido para deserialización y ORM (EF Core) */
    }

    public Product(Guid id, string name, string brand)
        : base(id)
    {
        SetName(name);
        SetBrand(brand);
    }

    public Product SetName(string name)
    {
        Check.NotNullOrWhiteSpace(name, nameof(name), maxLength: ProductConsts.MaxNameLength);
        Name = name.Trim();
        return this;
    }

    public Product SetBrand(string brand)
    {
        Check.NotNullOrWhiteSpace(brand, nameof(brand), maxLength: ProductConsts.MaxBrandLength);
        Brand = brand.Trim();
        return this;
    }
}
