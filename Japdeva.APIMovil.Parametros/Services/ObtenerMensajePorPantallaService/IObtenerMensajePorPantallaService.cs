using Microsoft.AspNetCore.Mvc;

namespace Japdeva.APIMovil.Parametros.Services.ObtenerMensajePorPantallaService
{
    /// <summary>
    /// Servicio para obtener mensajes desde la caché basándose en el identificador de pantalla.
    /// </summary>
    public interface IObtenerMensajePorPantallaService
    {
        /// <summary>
        /// Obtiene los mensajes activos asociados a una pantalla específica desde la caché.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="idPantalla">Identificador de la pantalla para filtrar los mensajes.</param>
        /// <returns>Una tarea que representa la operación asincrónica y contiene el resultado de la acción.</returns>
        Task<IActionResult> ObtenerMensajesPorPantallaAsync(string traceId, int idPantalla);
    }
}
