using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Common.Models;
using Japdeva.APIMovil.Reclamos.Models;


namespace Japdeva.APIMovil.Reclamos.Services.ObtenerReclamosPorUsuarioService
{
    /// <summary>
    /// Interfaz que define el contrato para el servicio de obtención de reclamos por usuario.
    /// Proporciona métodos para consultar reclamos filtrados por usuario y estado con paginación.
    /// </summary>
    public interface IObtenerReclamosPorUsuarioService
    {
        /// <summary>
        /// Obtiene una lista paginada de reclamos filtrados por usuario y estado.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="idUsuario">Identificador del usuario externo propietario de los reclamos.</param>
        /// <param name="idEstadoReclamo">Identificador del estado de reclamo a filtrar.</param>
        /// <param name="pagina">Número de página para la consulta paginada.</param>
        /// <returns>Un IActionResult con la lista paginada de reclamos que cumplen los criterios.</returns>
        Task<IActionResult> ObtenerReclamosPorUsuarioAsync(string traceId, int idUsuario, int idEstadoReclamo, int pagina);

        /// <summary>
        /// Realiza la consulta específica de reclamos por usuario y estado de forma asíncrona.
        /// Ejecuta la lógica de consulta directa a la base de datos y devuelve el modelo de respuesta estructurado.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="idUsuario">Identificador del usuario externo propietario de los reclamos.</param>
        /// <param name="idEstadoReclamo">Identificador del estado de reclamo a filtrar.</param>
        /// <param name="pagina">Número de página para la consulta paginada.</param>
        /// <returns>Modelo de respuesta con la lista paginada de entidades de reclamos y metadatos de paginación.</returns>
        Task<RespuestaListaModel<ReclamoRespuestaModel>> RealizarConsultaReclamoPorUsuarioAsync(string traceId, int idUsuario, int idEstadoReclamo, int pagina);
    }
}
