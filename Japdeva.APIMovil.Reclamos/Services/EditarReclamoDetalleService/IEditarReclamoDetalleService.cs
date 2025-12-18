using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Reclamos.Models;

namespace Japdeva.APIMovil.Reclamos.Services.EditarReclamoDetalleService
{
    /// <summary>
    /// Define el contrato para el servicio encargado de editar el detalle de un reclamo.
    /// </summary>
    public interface IEditarReclamoDetalleService
    {
        /// <summary>
        /// Edita un detalle de reclamo existente con validaciones y control transaccional.
        /// Actualiza el estado, descripción, proceso de devolución y usuario interno asignado.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="editarDetalleReclamoSolicitudModel">Modelo con los datos de la edición del detalle.</param>
        /// <returns>Resultado de la operación con la información del detalle editado.</returns>
        Task<IActionResult> EditarReclamoDetalleAsync(string traceId, EditarDetalleReclamoSolicitudModel editarDetalleReclamoSolicitudModel);
    }
}
