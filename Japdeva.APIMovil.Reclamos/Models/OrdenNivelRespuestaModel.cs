using Newtonsoft.Json;

namespace Japdeva.APIMovil.Reclamos.Models
{
    /// <summary>
    /// Modelo de respuesta que representa la información de un orden de nivel de proceso.
    /// Contiene datos sobre la estructura jerárquica de los procesos de reclamo,
    /// incluyendo niveles superiores e inferiores y configuraciones específicas.
    /// </summary>
    public class OrdenNivelRespuestaModel
    {
        /// <summary>
        /// Obtiene o establece el identificador único del orden de nivel.
        /// </summary>
        /// 
        [JsonProperty("idOrdenNivel")]
        public int IdOrdenNivel { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del nivel superior en la jerarquía de procesos.
        /// Define el nivel padre en la estructura de orden de procesos.
        /// </summary>
         
        [JsonProperty("idNivelSuperior")]
        public int IdNivelSuperior { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del nivel inferior en la jerarquía de procesos.
        /// Define el nivel hijo en la estructura de orden de procesos.
        /// </summary>
        [JsonProperty("idNivelInferior")]
        public int IdNivelInferior { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del departamento responsable de este nivel.
        /// </summary>
        [JsonProperty("idDepartamento")]
        public long IdDepartamento { get; set; }

        /// <summary>
        /// Obtiene o establece la descripción del departamento responsable del nivel de proceso.
        /// </summary>
        [JsonProperty("descripcionDepartamento")]
        public string DescripcionDepartamento { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece un valor que indica si este nivel permite devoluciones en el proceso.
        /// </summary>

        [JsonProperty("devolucionNivel")]
        public bool DevolucionNivel { get; set; }

        /// <summary>
        /// Obtiene o establece un valor que indica si este nivel representa la finalización del proceso.
        /// </summary>
         
        [JsonProperty("finalizacionProceso")]
        public bool FinalizacionProceso { get; set; }
    }
}
