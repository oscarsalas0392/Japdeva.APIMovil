using Microsoft.AspNetCore.Mvc;


namespace Japdeva.APIMovil.Reclamos.Services.ObtenerReclamosPorFechaIngresoService
{
    /// <summary>
    /// Define el contrato para el servicio que obtiene los reclamos por fecha de ingreso.
    /// </summary>
    public interface IObtenerReclamosPorFechaIngresoService
    {
        /// <summary>
        /// Obtiene los reclamos filtrados por el rango de fechas de ingreso especificado y la página solicitada.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad para la solicitud.</param>
        /// <param name="fechaInicio">Fecha de inicio del rango de búsqueda.</param>
        /// <param name="fechaFin">Fecha de fin del rango de búsqueda.</param>
        /// <param name="estadoReclamo">Estado del reclamo para filtrar.</param>
        /// <param name="pagina">Número de página para la paginación de resultados.</param>
        /// <returns>Una acción de resultado que contiene los reclamos encontrados.</returns>
        Task<IActionResult> ObtenerReclamosPorFechaIngresoAsync(string traceId, DateTime fechaInicio, DateTime fechaFin, int estadoReclamo, int pagina);
    }
}
