using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Reclamos.Models;

namespace Japdeva.APIMovil.Reclamos.Services.AgregarReclamoService
{
    /// <summary>
    /// Interfaz que define el contrato para el servicio de agregación de reclamos.
    /// Proporciona operaciones para crear y procesar nuevos reclamos con sus documentos asociados.
    /// </summary>
    public interface IAgregarReclamoService
    {
        /// <summary>
        /// Agrega un nuevo reclamo al sistema incluyendo sus documentos y detalles asociados.
        /// La operación se ejecuta dentro de una transacción para garantizar consistencia de datos.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="reclamo">Modelo con los datos del reclamo a crear.</param>
        /// <returns>Un IActionResult que contiene la respuesta con los datos del reclamo creado.</returns>
        Task<IActionResult> AgregarReclamoAsync(string traceId, AgregarReclamoSolicitudModel reclamo);
    }
}
