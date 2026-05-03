using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Reclamos.Models;

namespace Japdeva.APIMovil.Reclamos.Services.AgregarApelacionReclamoService
{
    /// <summary>
    /// Interfaz que define el contrato para el servicio de agregación de apelaciones de reclamo.
    /// </summary>
    public interface IAgregarApelacionReclamoService
    {
        /// <summary>
        /// Agrega una nueva apelación al sistema incluyendo sus documentos y detalles asociados.
        /// La operación se ejecuta dentro de una transacción para garantizar consistencia de datos.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="apelacion">Modelo con los datos de la apelación a crear.</param>
        /// <returns>Un IActionResult que contiene la respuesta con los datos de la apelación creada.</returns>
        Task<IActionResult> AgregarApelacionReclamoAsync(string traceId, AgregarApelacionReclamoSolicitudModel apelacion);
    }
}
