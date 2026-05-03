namespace Japdeva.APIMovil.Reclamos.Services.NotificarResolucionUsuarioService
{
    /// <summary>
    /// Define el contrato para notificar al usuario externo sobre la resolución de su reclamo.
    /// </summary>
    public interface INotificarResolucionUsuarioService
    {
        /// <summary>
        /// Notifica al usuario externo el resultado final de su reclamo mediante correo electrónico.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="idReclamo">Identificador del reclamo resuelto.</param>
        /// <param name="idUsuarioExterno">Identificador del usuario externo propietario del reclamo.</param>
        /// <param name="descripcionResolucion">Descripción de la resolución aplicada al reclamo.</param>
        /// <param name="estado">Estado final del reclamo (ej. Completado, Rechazado).</param>
        Task NotificarResolucionAsync(string traceId, long idReclamo, long idUsuarioExterno, string descripcionResolucion, string estado);
    }
}
