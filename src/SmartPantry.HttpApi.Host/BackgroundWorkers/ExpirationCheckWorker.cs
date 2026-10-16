using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmartPantry.Pantry;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Threading;
using Volo.Abp.Uow;

namespace SmartPantry.BackgroundWorkers;

/// <summary>
/// Worker periódico para procesar los vencimientos de la despensa (RF-15).
/// Ejecuta de forma independiente sin depender de sesión HTTP ni de ICurrentUser.
/// Resuelve el servicio de dominio desde el alcance de cada ejecución y delimita la unidad de trabajo.
/// </summary>
public class ExpirationCheckWorker : AsyncPeriodicBackgroundWorkerBase
{
    public ExpirationCheckWorker(
        AbpAsyncTimer timer,
        IServiceScopeFactory serviceScopeFactory)
        : base(timer, serviceScopeFactory)
    {
        // Periodo por defecto de 24 horas (86.400.000 ms)
        Timer.Period = 86400000;
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        Logger.LogInformation("Iniciando ejecución periódica de ExpirationCheckWorker...");

        try
        {
            // Resolver el servicio desde el alcance (scope) de cada ejecución, sin conservar dependencias scoped en el worker
            var warningManager = workerContext.ServiceProvider.GetRequiredService<IExpirationWarningManager>();
            var uowManager = workerContext.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();

            // Delimitar explícitamente una unidad de trabajo transaccional para las escrituras
            using (var uow = uowManager.Begin(requiresNew: true))
            {
                var result = await warningManager.ProcessExpirationWarningsAsync();
                await uow.CompleteAsync();

                Logger.LogInformation(
                    "ExpirationCheckWorker finalizado con éxito. Procesados: {Total}, Creadas: {Created}, Actualizadas: {Updated}, Desactivadas: {Deactivated}, Sin cambios: {Unchanged}.",
                    result.TotalProcessedItems,
                    result.CreatedWarnings,
                    result.UpdatedWarnings,
                    result.DeactivatedWarnings,
                    result.UnchangedWarnings);
            }
        }
        catch (Exception ex)
        {
            // Registrar errores sin detener las futuras ejecuciones del temporizador
            Logger.LogError(ex, "Ocurrió un error inesperado durante la ejecución periódica de vencimientos.");
        }
    }
}
