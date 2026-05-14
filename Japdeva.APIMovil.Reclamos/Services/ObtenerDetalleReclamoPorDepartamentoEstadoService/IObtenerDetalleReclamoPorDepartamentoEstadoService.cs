using Microsoft.AspNetCore.Mvc;

namespace Japdeva.APIMovil.Reclamos.Services.ObtenerDetalleReclamoPorDepartamentoEstadoService
{
    /// <summary>
    /// Interfaz para el servicio de obtención del último detalle de reclamo filtrado por departamento, estado detalle y reclamo.
    /// </summary>
    public interface IObtenerDetalleReclamoPorDepartamentoEstadoService
    {
        /// <summary>
        /// Obtiene el último detalle de reclamo que pertenece al departamento, estado detalle y reclamo indicados.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="idDepartamento">Identificador del departamento a filtrar.</param>
        /// <param name="idEstadoDetalleReclamo">Identificador del estado detalle de reclamo a filtrar.</param>
        /// <param name="idReclamo">Identificador del reclamo a filtrar.</param>
        /// <returns>Una tarea que representa la operación asincrónica y contiene el resultado de la acción.</returns>
        Task<IActionResult> ObtenerDetalleReclamoPorDepartamentoEstadoAsync(string traceId, long idDepartamento, int idEstadoDetalleReclamo, long idReclamo);
    }
}
