using Newtonsoft.Json;

namespace Japdeva.APIMovil.Colas.Models
{
    /// <summary>
    /// Modelo para actualizar un mensaje de solicitud.
    /// </summary>
    public class ActualizarMensajeSolicitudModel
    {
        /// <summary>
        /// Obtiene o establece el identificador único del mensaje.
        /// </summary>
        [JsonProperty("id")]
        public long Id { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador de seguimiento del mensaje.
        /// </summary>
        [JsonProperty("traceId")]
        public string TraceId { get; set; } = string.Empty;
    }
}