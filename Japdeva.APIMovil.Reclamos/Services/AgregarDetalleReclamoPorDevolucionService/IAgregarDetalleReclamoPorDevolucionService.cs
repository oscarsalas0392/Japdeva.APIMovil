namespace Japdeva.APIMovil.Reclamos.Services.AgregarDetalleReclamoPorDevolucionService
{
    /// <summary>
    /// Define el contrato para agregar un detalle de reclamo por devolución.
    /// </summary>
    public interface IAgregarDetalleReclamoPorDevolucionService
    {
        /// <summary>
        /// Agrega un detalle de reclamo por devolución utilizando el identificador de seguimiento, el identificador del reclamo y el identificador del proceso de devolución.
        /// </summary>
        /// <param name="traceId">Identificador de seguimiento para la operación.</param>
        /// <param name="idReclamo">Identificador único del reclamo.</param>
        /// <param name="idDevolucionProceso">Identificador del proceso de devolución.</param>
        Task AgregarDetalleReclamoPorDevolucionAsync(string traceId, long idReclamo, int idDevolucionProceso);
    }
}
