namespace Japdeva.APIMovil.Colas.Models
{
    /// <summary>
    /// Modelo para actualizar mensajes en cola.
    /// </summary>
    public class ActualizarMensajeModel
    {
        /// <summary>
        /// Obtiene o establece el identificador único del mensaje a actualizar.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Obtiene o establece el mensaje de error.
        /// </summary>
        public string MensajeError { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece el identificador de trazabilidad.
        /// </summary>
        public string TraceId { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece metadatos adicionales en formato JSON.
        /// </summary>
        public string Metadatos { get; set; } = string.Empty;
    }
}