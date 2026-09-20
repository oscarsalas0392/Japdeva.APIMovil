namespace Japdeva.APIMovil.Reclamos.Services.EditarDepartamentoReclamoService
{
    /// <summary>
    /// Define la funcionalidad para editar el departamento asociado a un reclamo.
    /// </summary>
    public interface IEditarDepartamentoReclamoService
    {
        /// <summary>
        /// Edita el departamento de un reclamo existente.
        /// </summary>
        /// <param name="traceId">Identificador de traza para seguimiento.</param>
        /// <param name="idReclamo">Identificador del reclamo a modificar.</param>
        /// <param name="idDepartamentoReclamo">Nuevo identificador del departamento a asociar al reclamo.</param>
        Task EditarDepartamentoReclamoAsync(string traceId, long idReclamo, long idDepartamentoReclamo);
    }
}
