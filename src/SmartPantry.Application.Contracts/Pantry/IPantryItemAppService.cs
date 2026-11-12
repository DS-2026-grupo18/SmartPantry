using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
namespace SmartPantry.Pantry;

public interface IPantryItemAppService : IApplicationService
{
    Task<PagedResultDto<PantryItemDto>> GetListAsync(GetPantryItemsInputDto input);
    Task<PantryItemDto> UpdateAsync(Guid id, UpdatePantryItemDto input);
    Task<PantryItemDto> ConsumeAsync(Guid id);
    Task DeleteAsync(Guid id);
}