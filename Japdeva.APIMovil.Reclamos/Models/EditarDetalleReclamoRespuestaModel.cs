using Newtonsoft.Json;

namespace Japdeva.APIMovil.Reclamos.Models
{
    /// <summary>
    /// Modelo de respuesta para la edición de detalles de reclamo.
    /// Contiene la información resultante después de editar un detalle de reclamo.
    /// </summary>
    public class EditarDetalleReclamoRespuestaModel
    {
        /// <summary>
        /// Obtiene o establece el identificador único del detalle del reclamo editado.
        /// </summary>
        [JsonProperty("idDetalleReclamo")]
        public long Id { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del estado actual del detalle del reclamo después de la edición.
        /// </summary>
        [JsonProperty("idEstadoActual")]
        public int IdEstadoDetalleReclamo { get; set; }

        /// <summary>
        /// Obtiene o establece la fecha y hora en que se realizó la edición del detalle.
        /// </summary>
        [JsonProperty("fechaEdicion")]
        public DateTime FechaEdicion { get; set; }
    }
}
