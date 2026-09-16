using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace SmartPantry.Products;

public class ProductAppService : SmartPantryAppService, IProductAppService
{
    private readonly IRepository<Product, Guid> _productRepository;

    public ProductAppService(IRepository<Product, Guid> productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto input)
    {
        var product = new Product(
            GuidGenerator.Create(),
            input.Name,
            input.Brand
        );

        await _productRepository.InsertAsync(product, autoSave: true);

        return MapToDto(product);
    }

    public async Task<ProductDto> GetAsync(Guid id)
    {
        var product = await _productRepository.GetAsync(id);

        return MapToDto(product);
    }

    private static ProductDto MapToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Brand = product.Brand,
            CreationTime = product.CreationTime,
            CreatorId = product.CreatorId,
            LastModificationTime = product.LastModificationTime,
            LastModifierId = product.LastModifierId
        };
    }
}
