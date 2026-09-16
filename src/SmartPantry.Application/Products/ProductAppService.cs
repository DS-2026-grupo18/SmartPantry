using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SmartPantry.Products;

public class ProductAppService :
    CrudAppService<
        Product,
        ProductDto,
        Guid,
        PagedAndSortedResultRequestDto,
        CreateUpdateProductDto>,
    IProductAppService
{
    public ProductAppService(IRepository<Product, Guid> repository)
        : base(repository)
    {
    }

    protected override Product MapToEntity(CreateUpdateProductDto createInput)
    {
        return new Product(
            GuidGenerator.Create(),
            createInput.Name,
            createInput.Brand
        );
    }

    protected override void MapToEntity(CreateUpdateProductDto updateInput, Product entity)
    {
        entity.Update(updateInput.Name, updateInput.Brand);
    }
}

