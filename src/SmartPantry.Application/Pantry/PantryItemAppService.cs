using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SmartPantry.Products;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;
namespace SmartPantry.Pantry;

[Authorize]
public class PantryItemAppService : ApplicationService, IPantryItemAppService
{
    private readonly IRepository<PantryItem, Guid> _pantryRepository;
    private readonly IRepository<Product, Guid> _productRepository;
    public PantryItemAppService(
        IRepository<PantryItem, Guid> pantryRepository,
        IRepository<Product, Guid> productRepository)
    {
        _pantryRepository = pantryRepository;
        _productRepository = productRepository;
    }
    public async Task<PagedResultDto<PantryItemDto>> GetListAsync(GetPantryItemsInputDto input)
    {
        var userId = CurrentUser.GetId();
        var query = await _pantryRepository.GetQueryableAsync();
        var misItems = query.Where(x => x.UserId == userId);
        var totalCount = misItems.Count();
        var itemsPaginados = misItems
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToList();
        var listaDto = new List<PantryItemDto>();
        foreach (var item in itemsPaginados)
        {
            var producto = await _productRepository.FindAsync(item.ProductId);
            var dto = new PantryItemDto();
            dto.Id = item.Id;
            dto.UserId = item.UserId;
            dto.ProductId = item.ProductId;
            dto.ProductName = producto != null ? producto.Name : "Sin nombre";
            dto.Status = item.Status;
            dto.ExpirationDate = item.ExpirationDate;
            dto.Quantity = item.Quantity;
            dto.Unit = item.Unit;
            listaDto.Add(dto);
        }
        return new PagedResultDto<PantryItemDto>(totalCount, listaDto);
    }
    public async Task<PantryItemDto> UpdateAsync(Guid id, UpdatePantryItemDto input)
    {
        var item = await ObtenerItemPropio(id);
        item.SetQuantity(input.Quantity);
        item.SetUnit(input.Unit);
        item.SetExpirationDate(input.ExpirationDate);
        await _pantryRepository.UpdateAsync(item);
        var producto = await _productRepository.FindAsync(item.ProductId);
        return ArmarDto(item, producto != null ? producto.Name : "Sin nombre");
    }
    public async Task<PantryItemDto> ConsumeAsync(Guid id)
    {
        var item = await ObtenerItemPropio(id);
        item.MarkAsConsumed();
        await _pantryRepository.UpdateAsync(item);
        var producto = await _productRepository.FindAsync(item.ProductId);
        return ArmarDto(item, producto != null ? producto.Name : "Sin nombre");
    }
    public async Task DeleteAsync(Guid id)
    {
        var item = await ObtenerItemPropio(id);
        await _pantryRepository.DeleteAsync(item);
    }
    private async Task<PantryItem> ObtenerItemPropio(Guid id)
    {
        var userId = CurrentUser.GetId();
        var item = await _pantryRepository.FindAsync(id);
        if (item == null || item.UserId != userId)
        {
            throw new EntityNotFoundException(typeof(PantryItem), id);
        }
        return item;
    }
    private PantryItemDto ArmarDto(PantryItem item, string nombreProducto)
    {
        var dto = new PantryItemDto();
        dto.Id = item.Id;
        dto.UserId = item.UserId;
        dto.ProductId = item.ProductId;
        dto.ProductName = nombreProducto;
        dto.Status = item.Status;
        dto.ExpirationDate = item.ExpirationDate;
        dto.Quantity = item.Quantity;
        dto.Unit = item.Unit;
        return dto;
    }
}