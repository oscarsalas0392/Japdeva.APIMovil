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

        /// <summary>
        /// Indica si este estado permite continuar el proceso al siguiente nivel.
        /// </summary>
        [JsonProperty("continuaProceso")]
        public bool ContinuaProceso { get; set; }

        /// <summary>
        /// Indica si este estado rechaza el proceso.
        /// </summary>
        [JsonProperty("rechazaProceso")]
        public bool RechazaProceso { get; set; }

        /// <summary>
        /// Indica si este estado implica devolución a un nivel anterior.
        /// </summary>
        [JsonProperty("devolucionProceso")]
        public bool DevolucionProceso { get; set; }

        /// <summary>
        /// Indica si este estado finaliza el proceso completamente.
        /// </summary>
        [JsonProperty("finalizarProceso")]
        public bool FinalizarProceso { get; set; }
    }
}
