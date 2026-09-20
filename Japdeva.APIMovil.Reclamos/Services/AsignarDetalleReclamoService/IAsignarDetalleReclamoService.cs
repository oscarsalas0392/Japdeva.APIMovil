using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Reclamos.Models;

namespace Japdeva.APIMovil.Reclamos.Services.AsignarDetalleReclamoService
{
    /// <summary>
    /// Define la interfaz para asignar un detalle de reclamo a un usuario interno.
    /// </summary>
    public interface IAsignarDetalleReclamoService
    {
        /// <summary>
        /// Asigna el detalle de reclamo indicado al usuario interno especificado,
        /// cambiando su estado a En Proceso. Solo se permite si el detalle está Pendiente.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="solicitud">Datos de la asignación: ID del detalle e ID del usuario interno.</param>
        /// <returns>Un IActionResult con el resultado de la operación.</returns>
        Task<IActionResult> AsignarDetalleReclamoAsync(string traceId, AsignarDetalleReclamoSolicitudModel solicitud);
    }
}
