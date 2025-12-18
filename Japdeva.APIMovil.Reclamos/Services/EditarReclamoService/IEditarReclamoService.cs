using System.Threading.Tasks;


namespace Japdeva.APIMovil.Reclamos.Services.EditarReclamoService
{
    /// <summary>
    /// Define la interfaz para el servicio de edición de reclamos.
    /// </summary>
    public interface IEditarReclamoService
    {
        /// <summary>
        /// Edita un reclamo existente.
        /// </summary>
        /// <param name="traceId">Identificador de traza.</param>
        /// <param name="idReclamo">Identificador del reclamo.</param>
        /// <param name="descripcionResolucion">Descripción de la resolución.</param>
        /// <param name="idEstadoReclamo">Identificador del estado del reclamo.</param>
        /// <returns>Una tarea que representa la operación asincrónica.</returns>
        Task EditarReclamoAsync(string traceId, long idReclamo, string descripcionResolucion, int idEstadoReclamo);
    }
}
