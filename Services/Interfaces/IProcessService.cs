using CompraPublicCercaMobile.Models;

namespace CompraPublicCercaMobile.Services.Interfaces;

/// <summary>
/// Consulta de procesos de contratación pública. Hoy lo implementa SercopProcessService
/// contra la API pública de SERCOP; si más adelante existe el backend propio
/// (docs/arquitectura.md), basta con otra implementación de esta interfaz.
/// </summary>
public interface IProcessService
{
    /// <summary>Busca una página de procesos con los filtros que aplica la API.</summary>
    /// <exception cref="ArgumentException">Si la palabra clave, la entidad o el proveedor son demasiado cortos.</exception>
    /// <exception cref="HttpRequestException">Si la API no responde correctamente.</exception>
    Task<PagedResult<Opportunity>> SearchAsync(
        ProcessSearchQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca aplicando además filtros que la API no tiene (provincia, monto). Descarga
    /// páginas desde <c>query.Page</c> hasta reunir <paramref name="minResults"/> procesos
    /// que cumplan, o hasta revisar <paramref name="maxPages"/> páginas.
    /// </summary>
    Task<FilteredSearchResult> SearchFilteredAsync(
        ProcessSearchQuery query,
        LocalProcessFilter filter,
        int minResults = 10,
        int maxPages = 5,
        CancellationToken cancellationToken = default);

    /// <summary>Devuelve el detalle de un proceso, o null si no existe.</summary>
    Task<ProcessDetail?> GetDetailAsync(string ocid, CancellationToken cancellationToken = default);
}
