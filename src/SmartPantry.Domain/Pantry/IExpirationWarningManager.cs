using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace SmartPantry.Pantry;

public interface IExpirationWarningManager : IDomainService
{
    /// <summary>
    /// Procesa de forma periódica e idempotente los vencimientos de la despensa.
    /// Utiliza explícitamente fecha de cálculo en formato UTC.
    /// Permite especificar una fecha de referencia controlada para pruebas automatizadas.
    /// </summary>
    /// <param name="referenceDateUtc">Fecha de referencia en UTC (por defecto Clock.Now en UTC).</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Resultado con métricas de advertencias creadas, actualizadas o desactivadas.</returns>
    Task<ExpirationWarningProcessResult> ProcessExpirationWarningsAsync(
        DateTime? referenceDateUtc = null,
        CancellationToken cancellationToken = default);
}
