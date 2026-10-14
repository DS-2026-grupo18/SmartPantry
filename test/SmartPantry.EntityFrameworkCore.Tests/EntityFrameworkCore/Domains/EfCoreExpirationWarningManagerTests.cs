using System;
using System.Threading.Tasks;
using SmartPantry.Pantry;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace SmartPantry.EntityFrameworkCore.Domains;

[Collection(SmartPantryTestConsts.CollectionDefinitionName)]
public class EfCoreExpirationWarningManagerTests : ExpirationWarningManager_Tests<SmartPantryEntityFrameworkCoreTestModule>
{
    [Fact]
    public async Task Should_Enforce_Unique_Index_On_PantryItemId_And_WarningType()
    {
        var warningRepo = GetRequiredService<IRepository<ExpirationWarning, Guid>>();
        var itemId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var warning1 = new ExpirationWarning(
            Guid.NewGuid(), itemId, userId, ExpirationWarningType.ExpirationNear, DateTime.UtcNow.Date, "Advertencia 1");
        await warningRepo.InsertAsync(warning1, autoSave: true);

        // Intentar insertar una segunda advertencia con la misma clave lógica (PantryItemId, WarningType)
        var warning2 = new ExpirationWarning(
            Guid.NewGuid(), itemId, userId, ExpirationWarningType.ExpirationNear, DateTime.UtcNow.Date, "Advertencia 2 duplicada");

        await Assert.ThrowsAnyAsync<Exception>(async () =>
        {
            await warningRepo.InsertAsync(warning2, autoSave: true);
        });
    }
}
