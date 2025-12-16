using Newtonsoft.Json;

namespace Japdeva.APIMovil.Reclamos.Models
{
    /// <summary>
    /// Modelo de respuesta que representa la información de un estado de detalle de reclamo.
    /// Contiene el identificador y la descripción del estado de un detalle específico de reclamo.
    /// </summary>
    public class EstadoDetalleReclamoRespuestaModel
    {
        /// <summary>
        /// Obtiene o establece el identificador único del estado del detalle de reclamo.
        /// </summary>
        /// 
        [JsonProperty("idEstadoDetalle")]
        public int IdEstadoDetalleReclamo { get; set; }

        /// <summary>
        /// Obtiene o establece la descripción del estado del detalle de reclamo.
        /// Proporciona información legible sobre el estado actual del detalle.
        /// </summary>
         
        [JsonProperty("descripcionEsadoDetalle")]
        public string DescripcionEstadoDetalleReclamo { get; set; } = string.Empty;
    }
}
