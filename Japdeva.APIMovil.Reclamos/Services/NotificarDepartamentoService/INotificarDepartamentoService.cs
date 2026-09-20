namespace Japdeva.APIMovil.Reclamos.Services.NotificarDepartamentoService
{
    /// <summary>
    /// Contrato para el servicio que notifica a los usuarios de un departamento sobre un nuevo reclamo.
    /// </summary>
    public interface INotificarDepartamentoService
    {
        /// <summary>
        /// Obtiene los correos del departamento y envía la plantilla de notificación interna a cada uno.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="idDepartamento">Identificador del departamento a notificar.</param>
        /// <param name="idReclamo">Identificador del reclamo recién ingresado.</param>
        /// <param name="idUsuarioExterno">Identificador del usuario externo que creó el reclamo.</param>
        Task NotificarNuevoReclamoAsync(string traceId, long idDepartamento, long idReclamo, long idUsuarioExterno);
    }
}
