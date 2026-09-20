using Microsoft.AspNetCore.Mvc;

namespace Japdeva.APIMovil.Parametros.Services.ObtenerOpcionPantallaPorPerfilService
{
    /// <summary>
    /// Define los métodos para obtener las opciones de pantalla activas por perfil de usuario.
    /// </summary>
    public interface IObtenerOpcionPantallaPorPerfilService
    {
        /// <summary>
        /// Obtiene las opciones de pantalla activas asociadas a un perfil específico.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="idPerfil">Identificador del perfil para filtrar las opciones.</param>
        /// <returns>Una tarea que representa la operación asincrónica y contiene el resultado de la acción.</returns>
        Task<IActionResult> ObtenerOpcionPantallaPorPerfilAsync(string traceId, int idPerfil);
    }
}
