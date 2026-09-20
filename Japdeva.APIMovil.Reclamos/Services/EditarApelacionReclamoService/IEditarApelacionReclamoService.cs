namespace Japdeva.APIMovil.Reclamos.Services.EditarApelacionReclamoService
{
    /// <summary>
    /// Define el contrato para el servicio de edición de apelaciones de reclamo.
    /// </summary>
    public interface IEditarApelacionReclamoService
    {
        /// <summary>
        /// Edita una apelación existente actualizando su descripción de resolución y estado.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="idApelacionReclamo">Identificador único de la apelación a editar.</param>
        /// <param name="descripcionResolucion">Nueva descripción de la resolución.</param>
        /// <param name="idEstadoReclamo">Nuevo identificador del estado.</param>
        Task EditarApelacionReclamoAsync(string traceId, long idApelacionReclamo, string descripcionResolucion, int idEstadoReclamo);
    }
}
