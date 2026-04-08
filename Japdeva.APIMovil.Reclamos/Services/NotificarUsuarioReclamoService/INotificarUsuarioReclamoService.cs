namespace Japdeva.APIMovil.Reclamos.Services.NotificarUsuarioReclamoService
{
    /// <summary>
    /// Contrato para el servicio que notifica al usuario externo cuando su reclamo ha sido registrado.
    /// </summary>
    public interface INotificarUsuarioReclamoService
    {
        /// <summary>
        /// Obtiene los datos del usuario y la plantilla en paralelo y publica el correo de confirmación del reclamo.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="idReclamo">Identificador del reclamo recién creado.</param>
        /// <param name="idUsuarioExterno">Identificador del usuario externo que creó el reclamo.</param>
        Task NotificarNuevoReclamoAsync(string traceId, long idReclamo, long idUsuarioExterno);
    }
}
