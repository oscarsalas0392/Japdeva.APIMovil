using Newtonsoft.Json;

namespace Japdeva.APIMovil.Reclamos.Models
{
    /// <summary>
    /// Modelo de respuesta que representa la información detallada de un detalle de reclamo.
    /// Contiene datos completos del detalle incluyendo información de usuario, departamento y estado.
    /// </summary>
    public class DetalleReclamoRespuestaModel
    {
        /// <summary>
        /// Obtiene o establece el identificador único del detalle de reclamo.
        /// </summary>
        [JsonProperty("id")]
        public long Id { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del reclamo al cual pertenece este detalle.
        /// </summary>

        [JsonProperty("idReclamo")]
        public long IdReclamo { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del usuario interno asignado al detalle.
        /// Puede ser nulo si no hay usuario asignado.
        /// </summary>
        [JsonProperty("idUsuarioInterno")]
        public long? IdUsuarioInterno { get; set; }

        /// <summary>
        /// Obtiene o establece el nombre del usuario interno asignado al detalle de reclamo.
        /// </summary>
 
        [JsonProperty("nombreUsuarioInterno")]
        public string NombreUsuarioInterno { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece el identificador del nivel de proceso asociado al detalle.
        /// </summary>
        [JsonProperty("idNivel")]
        public int IdNivelProceso { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del departamento responsable del detalle.
        /// </summary>
         
        [JsonProperty("idDepartamento")]
        public long IdDepartamento { get; set; }

        /// <summary>
        /// Obtiene o establece el nombre del departamento responsable del detalle de reclamo.
        /// </summary>
         
        [JsonProperty("nombreDepartamento")]
        public string NombreDepartamento { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece el identificador del estado actual del detalle de reclamo.
        /// </summary>
         
        [JsonProperty("idEstadoDetalleReclamo")]
        public int IdEstadoDetalleReclamo { get; set; }

        /// <summary>
        /// Obtiene o establece la descripción del estado actual del detalle de reclamo.
        /// </summary>
         
        [JsonProperty("descripcionEstadoDetalleReclamo")]
        public string DescripcionEstadoDetalleReclamo { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece la descripción o comentarios adicionales del detalle de reclamo.
        /// </summary>
        
        [JsonProperty("descripcion")]
        public string Descripcion { get; set; } = string.Empty;
    }
}
