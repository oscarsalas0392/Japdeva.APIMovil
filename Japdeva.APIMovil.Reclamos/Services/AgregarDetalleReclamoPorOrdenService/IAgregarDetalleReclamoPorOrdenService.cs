namespace Japdeva.APIMovil.Reclamos.Services.AgregarDetalleReclamoPorOrdenService
{
    /// <summary>
    /// Define el contrato para agregar un detalle de reclamo por orden.
    /// </summary>
    public interface IAgregarDetalleReclamoPorOrdenService
    {
        /// <summary>
        /// Agrega un detalle de reclamo asociado a una orden de proceso actual.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad de la operación.</param>
        /// <param name="idReclamo">Identificador del reclamo.</param>
        /// <param name="idOrdenProcesoActual">Identificador de la orden de proceso actual.</param>
        Task AgregarDetalleReclamoPorOrdenAsync(string traceId, long idReclamo, int idOrdenProcesoActual);
    }
}
