using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Common.Models;
using Japdeva.APIMovil.Reclamos.Models;

namespace Japdeva.APIMovil.Reclamos.Services.ObtenerApelacionesPorUsuarioService
{
    /// <summary>
    /// Interfaz que define el contrato para el servicio de obtención de apelaciones por usuario.
    /// </summary>
    public interface IObtenerApelacionesPorUsuarioService
    {
        /// <summary>
        /// Obtiene una lista paginada de apelaciones filtradas por usuario y estado.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="idUsuario">Identificador del usuario externo propietario de las apelaciones.</param>
        /// <param name="idEstadoReclamo">Identificador del estado a filtrar.</param>
        /// <param name="pagina">Número de página para la consulta paginada.</param>
        /// <returns>Un IActionResult con la lista paginada de apelaciones que cumplen los criterios.</returns>
        Task<IActionResult> ObtenerApelacionesPorUsuarioAsync(string traceId, int idUsuario, int idEstadoReclamo, int pagina);

        /// <summary>
        /// Realiza la consulta paginada de apelaciones para un usuario y estado específicos.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="idUsuario">Identificador del usuario externo.</param>
        /// <param name="idEstadoReclamo">Identificador del estado a filtrar.</param>
        /// <param name="pagina">Número de página para la consulta paginada.</param>
        /// <returns>Modelo de respuesta con la lista paginada de apelaciones y metadatos de paginación.</returns>
        Task<RespuestaListaModel<ApelacionReclamoRespuestaModel>> RealizarConsultaApelacionPorUsuarioAsync(string traceId, int idUsuario, int idEstadoReclamo, int pagina);
    }
}
