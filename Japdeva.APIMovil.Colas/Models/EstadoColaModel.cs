namespace Japdeva.APIMovil.Colas.Models
{
    /// <summary>
    /// Modelo para consultar el estado de una cola específica.
    /// </summary>
    public class EstadoColaModel
    {
        /// <summary>
        /// Obtiene o establece el nombre de la cola.
        /// </summary>
        public string NombreCola { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece el número total de mensajes en la cola.
        /// </summary>
        public int TotalMensajes { get; set; } = 0;

        /// <summary>
        /// Obtiene o establece el número de mensajes pendientes.
        /// </summary>
        public int MensajesPendientes { get; set; } = 0;

        /// <summary>
        /// Obtiene o establece el número de mensajes procesados.
        /// </summary>
        public int MensajesProcesados { get; set; } = 0;

        /// <summary>
        /// Obtiene o establece el número de mensajes fallidos.
        /// </summary>
        public int MensajesFallidos { get; set; } = 0;

        /// <summary>
        /// Obtiene o establece si la cola está activa.
        /// </summary>
        public bool EstaActiva { get; set; } = true;

        /// <summary>
        /// Obtiene o establece la fecha de última actualización.
        /// </summary>
        public DateTime UltimaActualizacion { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Obtiene o establece la fecha del último procesamiento.
        /// </summary>
        public DateTime? UltimoProcesamiento { get; set; }
    }
}