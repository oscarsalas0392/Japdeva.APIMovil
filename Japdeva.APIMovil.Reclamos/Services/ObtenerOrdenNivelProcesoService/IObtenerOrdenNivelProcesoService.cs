using Microsoft.AspNetCore.Mvc;

namespace Japdeva.APIMovil.Reclamos.Services.ObtenerOrdenNivelProcesoService
{
    /// <summary>
    /// Define la interfaz para obtener la orden del nivel de proceso.
    /// </summary>
    public interface IObtenerOrdenNivelProcesoService
    {
        /// <summary>
        /// Obtiene la orden del nivel de proceso de acuerdo al identificador del nivel superior y la página solicitada.
        /// </summary>
        /// <param name="traceId">Identificador de traza para seguimiento.</param>
        /// <param name="idNivelSuperior">Identificador del nivel superior.</param>
        /// <param name="pagina">Número de página solicitada.</param>
        /// <returns>Una tarea que representa la operación asincrónica y contiene el resultado de la acción.</returns>
        Task<IActionResult> ObtenerOrdenNivelProcesoAsync(string traceId, int idNivelSuperior, int pagina);
    }
}
