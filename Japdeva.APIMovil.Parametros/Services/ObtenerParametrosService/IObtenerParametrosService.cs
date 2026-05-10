using Microsoft.AspNetCore.Mvc;

namespace Japdeva.APIMovil.Parametros.Services.ObtenerParametrosService
{
    /// <summary>
    /// Interfaz que define el contrato para el servicio de obtención de múltiples parámetros a la vez.
    /// </summary>
    public interface IObtenerParametrosService
    {
        /// <summary>
        /// Obtiene una lista de parámetros activos por sus nombres desde la caché.
        /// Los nombres no encontrados son omitidos del resultado.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="nombres">Lista de nombres de los parámetros a obtener.</param>
        /// <returns>Una tarea que representa la operación asincrónica y contiene la lista de parámetros encontrados.</returns>
        Task<IActionResult> ObtenerParametrosPorNombresAsync(string traceId, List<string> nombres);
    }
}
