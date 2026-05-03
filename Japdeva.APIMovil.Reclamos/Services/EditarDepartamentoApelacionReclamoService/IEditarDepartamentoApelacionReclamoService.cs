namespace Japdeva.APIMovil.Reclamos.Services.EditarDepartamentoApelacionReclamoService
{
    /// <summary>
    /// Define el contrato para el servicio de edición del departamento de una apelación de reclamo.
    /// </summary>
    public interface IEditarDepartamentoApelacionReclamoService
    {
        /// <summary>
        /// Actualiza el departamento actualmente asignado a una apelación.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="idApelacionReclamo">Identificador de la apelación a modificar.</param>
        /// <param name="idDepartamento">Identificador del nuevo departamento a asignar.</param>
        Task EditarDepartamentoApelacionReclamoAsync(string traceId, long idApelacionReclamo, long idDepartamento);
    }
}
