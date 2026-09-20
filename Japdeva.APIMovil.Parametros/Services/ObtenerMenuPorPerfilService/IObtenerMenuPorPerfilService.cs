using Microsoft.AspNetCore.Mvc;

namespace Japdeva.APIMovil.Parametros.Services.ObtenerMenuPorPerfilService
{
    /// <summary>
    /// Servicio para obtener los menús asociados a un perfil específico desde la caché.
    /// </summary>
    public interface IObtenerMenuPorPerfilService
    {
        /// <summary>
        /// Obtiene la lista de menús activos asociados a un perfil específico desde la caché.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="idPerfil">Identificador del perfil para filtrar los menús.</param>
        /// <returns>Una tarea que representa la operación asincrónica y contiene el resultado de la acción.</returns>
        Task<IActionResult> ObtenerMenusPorPerfilAsync(string traceId, int idPerfil);
    }
}
