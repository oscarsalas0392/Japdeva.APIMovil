namespace Japdeva.APIMovil.Colas.Models
{
    /// <summary>
    /// Modelo para envío de mensajes a la cola.
    /// </summary>
    public class EnviarMensajeSolicitudModel
    {
        private const int PRIORIDAD_PREDETERMINADA = 2;

        /// <summary>
        /// Obtiene o establece el identificador único del mensaje.
        /// </summary>
        public int Id   { get; set; }

        /// <summary>
        /// Obtiene o establece el nombre de la cola de destino.
        /// </summary>
        public string NombreCola { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece el contenido del mensaje.
        /// </summary>
        public string ContenidoMensaje { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece la prioridad del mensaje (1=Alta, 2=Media, 3=Baja).
        /// </summary>
        public int Prioridad { get; set; } = PRIORIDAD_PREDETERMINADA;


        /// <summary>
        /// Obtiene o establece el identificador de trazabilidad.
        /// </summary>
        public string TraceId { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece metadatos adicionales.
        /// </summary>
        public string Metadatos { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece el mensaje de error asociado al procesamiento.
        /// </summary>
        public string MensajeError { get; set; } = string.Empty;
    }
}