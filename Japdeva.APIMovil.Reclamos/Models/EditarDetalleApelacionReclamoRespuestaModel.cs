using Newtonsoft.Json;

namespace Japdeva.APIMovil.Reclamos.Models
{
    /// <summary>
    /// Modelo de respuesta para la edición de detalles de apelación de reclamo.
    /// </summary>
    public class EditarDetalleApelacionReclamoRespuestaModel
    {
        /// <summary>
        /// Obtiene o establece el identificador único del detalle de apelación editado.
        /// </summary>
        [JsonProperty("idDetalleApelacionReclamo")]
        public long Id { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del estado actual del detalle tras la edición.
        /// </summary>
        [JsonProperty("idEstadoActual")]
        public int IdEstadoDetalleReclamo { get; set; }

        /// <summary>
        /// Obtiene o establece la fecha y hora en que se realizó la edición.
        /// </summary>
        [JsonProperty("fechaEdicion")]
        public DateTime FechaEdicion { get; set; }
    }
}
