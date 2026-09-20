using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Reclamos.Models;

namespace Japdeva.APIMovil.Reclamos.Services.EditarApelacionReclamoDetalleService
{
    /// <summary>
    /// Define el contrato para el servicio de edición de detalles de apelación de reclamo.
    /// </summary>
    public interface IEditarApelacionReclamoDetalleService
    {
        /// <summary>
        /// Edita un detalle de apelación existente con validaciones y control de workflow.
        /// Actualiza el estado, descripción y usuario interno, e invoca el motor de proceso si aplica.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="solicitud">Modelo con los datos de la edición del detalle.</param>
        /// <returns>Resultado de la operación con la información del detalle editado.</returns>
        Task<IActionResult> EditarApelacionReclamoDetalleAsync(string traceId, EditarDetalleApelacionReclamoSolicitudModel solicitud);
    }
}
