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
    // Variable privada donde guardamos el cliente externo inyectado
    private readonly IExternalProductCatalogClient _externalCatalogClient;

    // Constructor: recibe las herramientas necesarias (el repositorio interno y el cliente externo)
    public ProductAppService(
        IRepository<Product, Guid> repository,
        IExternalProductCatalogClient externalCatalogClient)
        : base(repository)
    {
        _externalCatalogClient = externalCatalogClient;
    }

    // Método solicitado en TP07 para consultar la API externa
    public async Task<ExternalProductDto?> GetByBarcodeAsync(GetProductByBarcodeDto input)
    {
        return await _externalCatalogClient.GetByBarcodeAsync(input.Barcode);
    }

    // Métodos de mapeo heredados del TP06 para la entidad interna
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