using Microsoft.AspNetCore.Mvc;

namespace Japdeva.APIMovil.Parametros.Services.ObtenerParametroService
{
    /// <summary>
    /// Interfaz para el servicio de obtención de parámetros del sistema.
    /// </summary>
    public interface IObtenerParametroService
    {
        /// <summary>
        /// Obtiene un parámetro activo por su nombre.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="nombre">Nombre del parámetro a obtener.</param>
        /// <returns>Una tarea que representa la operación asincrónica y contiene el resultado de la acción.</returns>
        Task<IActionResult> ObtenerParametroPorNombreAsync(string traceId, string nombre);
    }
}
