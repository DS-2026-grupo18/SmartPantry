using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace SmartPantry.Pantry;

public class ExpirationWarningManager : DomainService, IExpirationWarningManager
{
    private readonly IRepository<PantryItem, Guid> _pantryItemRepository;
    private readonly IRepository<ExpirationWarning, Guid> _warningRepository;

    public ExpirationWarningManager(
        IRepository<PantryItem, Guid> pantryItemRepository,
        IRepository<ExpirationWarning, Guid> warningRepository)
    {
        _pantryItemRepository = pantryItemRepository;
        _warningRepository = warningRepository;
    }

    public async Task<ExpirationWarningProcessResult> ProcessExpirationWarningsAsync(
        DateTime? referenceDateUtc = null,
        CancellationToken cancellationToken = default)
    {
        // Se explicita y mantiene el cálculo en tiempo universal coordinado (UTC)
        var nowUtc = (referenceDateUtc ?? Clock.Now).ToUniversalTime().Date;
        var thresholdLimit = nowUtc.AddDays(PantryConsts.DefaultThresholdDays);

        var items = await _pantryItemRepository.GetListAsync(cancellationToken: cancellationToken);
        var existingWarnings = await _warningRepository.GetListAsync(
            w => w.WarningType == ExpirationWarningType.ExpirationNear,
            cancellationToken: cancellationToken);

        var warningsByItem = existingWarnings
            .GroupBy(w => w.PantryItemId)
            .ToDictionary(g => g.Key, g => g.First());

        var result = new ExpirationWarningProcessResult
        {
            ReferenceDateUtc = nowUtc,
            TotalProcessedItems = items.Count
        };

        foreach (var item in items)
        {
            bool shouldBeActive = false;
            string? warningMessage = null;

            if (item.Status == PantryItemStatus.Active && item.ExpirationDate.HasValue)
            {
                var expDate = item.ExpirationDate.Value.Date;
                if (expDate <= thresholdLimit)
                {
                    shouldBeActive = true;
                    if (expDate < nowUtc)
                    {
                        var diffDays = (nowUtc - expDate).Days;
                        warningMessage = $"El ítem venció el {expDate:yyyy-MM-dd} (hace {diffDays} día{(diffDays == 1 ? "" : "s")}).";
                    }
                    else if (expDate == nowUtc)
                    {
                        warningMessage = $"El ítem vence hoy ({expDate:yyyy-MM-dd}).";
                    }
                    else
                    {
                        var diffDays = (expDate - nowUtc).Days;
                        warningMessage = $"El ítem vence el {expDate:yyyy-MM-dd} (en {diffDays} día{(diffDays == 1 ? "" : "s")}).";
                    }
                }
            }

            warningsByItem.TryGetValue(item.Id, out var existingWarning);

            if (shouldBeActive)
            {
                if (existingWarning == null)
                {
                    var newWarning = new ExpirationWarning(
                        GuidGenerator.Create(),
                        item.Id,
                        item.UserId,
                        ExpirationWarningType.ExpirationNear,
                        item.ExpirationDate,
                        warningMessage!
                    );

                    await _warningRepository.InsertAsync(newWarning, autoSave: true, cancellationToken: cancellationToken);
                    warningsByItem[item.Id] = newWarning;
                    result.CreatedWarnings++;
                }
                else
                {
                    bool dateChanged = existingWarning.ExpirationDate != item.ExpirationDate?.Date;
                    bool messageChanged = existingWarning.Message != warningMessage;
                    bool wasInactive = !existingWarning.IsActive;

                    if (wasInactive)
                    {
                        existingWarning.Activate(item.ExpirationDate, warningMessage!);
                        await _warningRepository.UpdateAsync(existingWarning, autoSave: true, cancellationToken: cancellationToken);
                        result.UpdatedWarnings++;
                    }
                    else if (dateChanged || messageChanged)
                    {
                        existingWarning.UpdateExpirationDate(item.ExpirationDate, warningMessage!);
                        await _warningRepository.UpdateAsync(existingWarning, autoSave: true, cancellationToken: cancellationToken);
                        result.UpdatedWarnings++;
                    }
                    else
                    {
                        result.UnchangedWarnings++;
                    }
                }
            }
            else
            {
                if (existingWarning != null && existingWarning.IsActive)
                {
                    existingWarning.Deactivate("Desactivada: ítem consumido, sin fecha o fuera de umbral");
                    await _warningRepository.UpdateAsync(existingWarning, autoSave: true, cancellationToken: cancellationToken);
                    result.DeactivatedWarnings++;
                }
                else if (existingWarning != null)
                {
                    result.UnchangedWarnings++;
                }
            }
        }

        return result;
    }
}
