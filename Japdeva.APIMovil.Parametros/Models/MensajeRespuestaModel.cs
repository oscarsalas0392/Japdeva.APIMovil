using Newtonsoft.Json;

namespace Japdeva.APIMovil.Parametros.Models
{
    /// <summary>
    /// Modelo de respuesta que representa un mensaje asociado a una pantalla.
    /// Contiene información sobre el mensaje, su tipo y la pantalla asociada.
    /// </summary>
    public class MensajeRespuestaModel
    {
        /// <summary>
        /// Obtiene o establece el identificador único del mensaje.
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        /// Obtiene o establece la descripción del mensaje.
        /// </summary>
        [JsonProperty("descripcion")]
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece el identificador del tipo de mensaje asociado.
        /// </summary>
        [JsonProperty("idTipoMensaje")]
        public int IdTipoMensaje { get; set; }

        /// <summary>
        /// Obtiene o establece la descripción del tipo de mensaje asociado.
        /// </summary>
        [JsonProperty("descripcionTipoMensaje")]
        public string DescripcionTipoMensaje { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece el identificador de la pantalla asociada al mensaje.
        /// </summary>
        [JsonProperty("idPantalla")]
        public int IdPantalla { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del usuario interno asociado al mensaje.
        /// </summary>
        [JsonProperty("idUsuarioInterno")]
        public long? IdUsuarioInterno { get; set; }
    }
}
