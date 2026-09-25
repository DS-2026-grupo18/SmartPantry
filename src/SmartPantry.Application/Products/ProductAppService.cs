using System;
using System.Threading.Tasks;
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
    private readonly IExternalProductCatalogClient _externalCatalogClient;

    public ProductAppService(
        IRepository<Product, Guid> repository,
        IExternalProductCatalogClient externalCatalogClient)
        : base(repository)
    {
        _externalCatalogClient = externalCatalogClient;
    }

    public async Task<ExternalProductResultDto> GetByBarcodeAsync(GetProductByBarcodeDto input)
    {
        try
        {
            var product = await _externalCatalogClient.GetByBarcodeAsync(input.Barcode);
            if (product == null)
            {
                return ExternalProductResultDto.NotFound();
            }

            return ExternalProductResultDto.Found(product);
        }
        catch (ExternalCatalogRateLimitException ex)
        {
            return ExternalProductResultDto.RateLimit(ex.Message);
        }
        catch (ExternalCatalogUnavailableException ex)
        {
            return ExternalProductResultDto.ServiceUnavailable(ex.Message);
        }
        catch (Exception)
        {
            return ExternalProductResultDto.ServiceUnavailable();
        }
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
