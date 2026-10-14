using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Modularity;
using Xunit;

namespace SmartPantry.Pantry;

public abstract class ExpirationWarningManager_Tests<TStartupModule> : SmartPantryDomainTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IExpirationWarningManager _warningManager;
    private readonly IRepository<PantryItem, Guid> _pantryItemRepository;
    private readonly IRepository<ExpirationWarning, Guid> _warningRepository;

    protected ExpirationWarningManager_Tests()
    {
        _warningManager = GetRequiredService<IExpirationWarningManager>();
        _pantryItemRepository = GetRequiredService<IRepository<PantryItem, Guid>>();
        _warningRepository = GetRequiredService<IRepository<ExpirationWarning, Guid>>();
    }

    private async Task ClearPantryDataAsync()
    {
        await _warningRepository.DeleteAsync(x => true, autoSave: true);
        await _pantryItemRepository.DeleteAsync(x => true, autoSave: true);
    }

    [Fact]
    public async Task Should_Process_Expiration_Warnings_With_Controlled_Utc_Date_And_All_Scenarios()
    {
        await ClearPantryDataAsync();

        // Arrange - Se utiliza explícitamente fecha en formato UTC
        var referenceDateUtc = new DateTime(2026, 10, 14, 0, 0, 0, DateTimeKind.Utc);
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        // 1. Dentro del umbral (a vencer en 2 días: 16 de octubre)
        var itemWithinThreshold = await _pantryItemRepository.InsertAsync(
            new PantryItem(Guid.NewGuid(), userId, productId, referenceDateUtc.AddDays(2)),
            autoSave: true);

        // 2. Día límite del umbral (exactamente a vencer en 3 días: 17 de octubre)
        var itemBoundaryDay = await _pantryItemRepository.InsertAsync(
            new PantryItem(Guid.NewGuid(), userId, productId, referenceDateUtc.AddDays(3)),
            autoSave: true);

        // 3. Fuera del umbral (a vencer en 5 días: 19 de octubre)
        var itemOutsideThreshold = await _pantryItemRepository.InsertAsync(
            new PantryItem(Guid.NewGuid(), userId, productId, referenceDateUtc.AddDays(5)),
            autoSave: true);

        // 4. Vencido (venció hace 1 día: 13 de octubre)
        var itemExpired = await _pantryItemRepository.InsertAsync(
            new PantryItem(Guid.NewGuid(), userId, productId, referenceDateUtc.AddDays(-1)),
            autoSave: true);

        // 5. Sin fecha de vencimiento
        var itemWithoutDate = await _pantryItemRepository.InsertAsync(
            new PantryItem(Guid.NewGuid(), userId, productId, null),
            autoSave: true);

        // 6. Ya consumido (incluso teniendo fecha dentro del umbral)
        var itemConsumed = await _pantryItemRepository.InsertAsync(
            new PantryItem(Guid.NewGuid(), userId, productId, referenceDateUtc.AddDays(1), PantryItemStatus.Consumed),
            autoSave: true);

        // Act - Ejecución con fecha controlada sin depender del temporizador
        var result = await _warningManager.ProcessExpirationWarningsAsync(referenceDateUtc);

        // Assert - Métricas del procesamiento
        result.ReferenceDateUtc.ShouldBe(referenceDateUtc);
        result.CreatedWarnings.ShouldBe(3); // Dentro del umbral, Día límite y Vencido

        var warnings = await _warningRepository.GetListAsync();

        // 1. Debe haber advertencia activa para ítem dentro del umbral
        var warningWithin = warnings.FirstOrDefault(w => w.PantryItemId == itemWithinThreshold.Id);
        warningWithin.ShouldNotBeNull();
        warningWithin.IsActive.ShouldBeTrue();
        warningWithin.Message.ShouldContain("en 2 días");

        // 2. Debe haber advertencia activa para ítem en el día límite
        var warningBoundary = warnings.FirstOrDefault(w => w.PantryItemId == itemBoundaryDay.Id);
        warningBoundary.ShouldNotBeNull();
        warningBoundary.IsActive.ShouldBeTrue();
        warningBoundary.Message.ShouldContain("en 3 días");

        // 3. NO debe haber advertencia para ítem fuera del umbral
        warnings.Any(w => w.PantryItemId == itemOutsideThreshold.Id).ShouldBeFalse();

        // 4. Debe haber advertencia activa para ítem vencido
        var warningExpired = warnings.FirstOrDefault(w => w.PantryItemId == itemExpired.Id);
        warningExpired.ShouldNotBeNull();
        warningExpired.IsActive.ShouldBeTrue();
        warningExpired.Message.ShouldContain("venció el");

        // 5. NO debe haber advertencia para ítem sin fecha
        warnings.Any(w => w.PantryItemId == itemWithoutDate.Id).ShouldBeFalse();

        // 6. NO debe haber advertencia para ítem consumido
        warnings.Any(w => w.PantryItemId == itemConsumed.Id).ShouldBeFalse();
    }

    [Fact]
    public async Task Should_Be_Idempotent_When_Executed_Multiple_Times()
    {
        await ClearPantryDataAsync();

        // Arrange
        var referenceDateUtc = new DateTime(2026, 10, 14, 0, 0, 0, DateTimeKind.Utc);
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var item1 = await _pantryItemRepository.InsertAsync(
            new PantryItem(Guid.NewGuid(), userId, productId, referenceDateUtc.AddDays(1)),
            autoSave: true);

        var item2 = await _pantryItemRepository.InsertAsync(
            new PantryItem(Guid.NewGuid(), userId, productId, referenceDateUtc.AddDays(2)),
            autoSave: true);

        // Act 1: Primera ejecución
        var firstResult = await _warningManager.ProcessExpirationWarningsAsync(referenceDateUtc);
        firstResult.CreatedWarnings.ShouldBe(2);

        var initialWarnings = await _warningRepository.GetListAsync();
        initialWarnings.Count(w => w.PantryItemId == item1.Id).ShouldBe(1);
        initialWarnings.Count(w => w.PantryItemId == item2.Id).ShouldBe(1);

        // Act 2: Segunda ejecución consecutiva sobre los mismos datos
        var secondResult = await _warningManager.ProcessExpirationWarningsAsync(referenceDateUtc);

        // Assert - La regla es idempotente: no duplica registros
        secondResult.CreatedWarnings.ShouldBe(0);
        secondResult.UpdatedWarnings.ShouldBe(0);
        secondResult.DeactivatedWarnings.ShouldBe(0);
        secondResult.UnchangedWarnings.ShouldBeGreaterThanOrEqualTo(2);

        var finalWarnings = await _warningRepository.GetListAsync();
        finalWarnings.Count(w => w.PantryItemId == item1.Id).ShouldBe(1);
        finalWarnings.Count(w => w.PantryItemId == item2.Id).ShouldBe(1);
    }

    [Fact]
    public async Task Should_Update_Deactivate_And_Reactivate_Same_Warning_On_Date_Changes()
    {
        await ClearPantryDataAsync();

        // Arrange
        var referenceDateUtc = new DateTime(2026, 10, 14, 0, 0, 0, DateTimeKind.Utc);
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var item = await _pantryItemRepository.InsertAsync(
            new PantryItem(Guid.NewGuid(), userId, productId, referenceDateUtc.AddDays(2)),
            autoSave: true);

        // 1. Creación inicial
        var res1 = await _warningManager.ProcessExpirationWarningsAsync(referenceDateUtc);
        res1.CreatedWarnings.ShouldBe(1);

        var warning1 = (await _warningRepository.GetListAsync(w => w.PantryItemId == item.Id)).Single();
        var originalWarningId = warning1.Id;
        warning1.IsActive.ShouldBeTrue();
        warning1.ExpirationDate.ShouldBe(referenceDateUtc.AddDays(2).Date);

        // 2. Actualización de fecha (sigue dentro del umbral: a vencer en 1 día)
        item.SetExpirationDate(referenceDateUtc.AddDays(1));
        await _pantryItemRepository.UpdateAsync(item, autoSave: true);

        var res2 = await _warningManager.ProcessExpirationWarningsAsync(referenceDateUtc);
        res2.UpdatedWarnings.ShouldBe(1);

        var warning2 = (await _warningRepository.GetListAsync(w => w.PantryItemId == item.Id)).Single();
        warning2.Id.ShouldBe(originalWarningId); // Misma entidad
        warning2.IsActive.ShouldBeTrue();
        warning2.ExpirationDate.ShouldBe(referenceDateUtc.AddDays(1).Date);
        warning2.Message.ShouldContain("en 1 día");

        // 3. Desactivación: se extiende la fecha fuera del umbral (a vencer en 10 días)
        item.SetExpirationDate(referenceDateUtc.AddDays(10));
        await _pantryItemRepository.UpdateAsync(item, autoSave: true);

        var res3 = await _warningManager.ProcessExpirationWarningsAsync(referenceDateUtc);
        res3.DeactivatedWarnings.ShouldBe(1);

        var warning3 = (await _warningRepository.GetListAsync(w => w.PantryItemId == item.Id)).Single();
        warning3.Id.ShouldBe(originalWarningId); // Misma entidad conservada
        warning3.IsActive.ShouldBeFalse();
        warning3.ResolvedTime.ShouldNotBeNull();

        // 4. Reactivación: se vuelve a acortar la fecha dentro del umbral (hoy mismo)
        item.SetExpirationDate(referenceDateUtc);
        await _pantryItemRepository.UpdateAsync(item, autoSave: true);

        var res4 = await _warningManager.ProcessExpirationWarningsAsync(referenceDateUtc);
        res4.UpdatedWarnings.ShouldBe(1);

        var warning4 = (await _warningRepository.GetListAsync(w => w.PantryItemId == item.Id)).Single();
        warning4.Id.ShouldBe(originalWarningId); // Misma entidad reactivada sin duplicar
        warning4.IsActive.ShouldBeTrue();
        warning4.ResolvedTime.ShouldBeNull();
        warning4.ExpirationDate.ShouldBe(referenceDateUtc.Date);
        warning4.Message.ShouldContain("vence hoy");
    }
}
