namespace Japdeva.APIMovil.Usuarios.Services.NotificarContrasenaTemporalService
{
    /// <summary>
    /// Define el contrato para notificar al usuario su contraseña temporal por correo electrónico.
    /// </summary>
    public interface INotificarContrasenaTemporalService
    {
        /// <summary>
        /// Obtiene la plantilla de correo y publica el envío con la contraseña temporal generada.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="nombre">Nombre del usuario destinatario.</param>
        /// <param name="correo">Dirección de correo del usuario.</param>
        /// <param name="contrasenaTemporal">Contraseña temporal generada para el usuario.</param>
        Task NotificarAsync(string traceId, string nombre, string correo, string contrasenaTemporal);
    }
}
