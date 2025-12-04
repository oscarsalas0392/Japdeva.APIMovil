namespace Japdeva.APIMovil.Colas.Models
{
    /// <summary>
    /// Modelo de respuesta para mensajes de colas
    /// </summary>
    public class MensajeColasRespuestaModel
    {
        /// <summary>
        /// Identificador único del mensaje
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Identificador de RPC
        /// </summary>
        public string IdRpc { get; set; } = string.Empty;

        /// <summary>
        /// Identificador de la cola
        /// </summary>
        public long Cola { get; set; }

        /// <summary>
        /// Identificador de trazabilidad
        /// </summary>
        public string TraceId { get; set; } = string.Empty;

        /// <summary>
        /// Contenido del mensaje
        /// </summary>
        public string Mensaje { get; set; } = string.Empty;

        /// <summary>
        /// Estado del mensaje
        /// </summary>
        public int Estado { get; set; } 

        /// <summary>
        /// Metadatos adicionales del mensaje
        /// </summary>
        public string MetaDatos { get; set; } = string.Empty;

        /// <summary>
        /// Indica si el TraceId es diferente
        /// </summary>
        public bool TraceIdDiferente { get; set; } = false;

    }
}