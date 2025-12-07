using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Reclamos.Models;
using System.Threading.Tasks;

namespace Japdeva.APIMovil.Reclamos.Services.ObtenerReclamosPorUsuarioService
{
    /// <summary>
    /// Define la interfaz para el servicio de obtención de reclamos por usuario.
    /// </summary>
    public interface IObtenerReclamosPorUsuarioService
    {
        /// <summary>
        /// Obtiene los reclamos asociados a un usuario según el estado y la página solicitada.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="idUsuario">Identificador del usuario externo.</param>
        /// <param name="idEstadoReclamo">Identificador del estado del reclamo.</param>
        /// <param name="pagina">Número de página para paginación.</param>
        /// <returns>Resultado de la acción con la lista de reclamos.</returns>
        Task<IActionResult> ObtenerReclamosPorUsuario(string traceId, int idUsuario, int idEstadoReclamo, int pagina);
    }
}