using Newtonsoft.Json;

namespace Japdeva.APIMovil.Reclamos.Models
{
    /// <summary>
    /// Modelo que representa la respuesta de un reclamo, incluyendo información relevante como estado, usuario y departamento.
    /// </summary>
    public class ReclamoRespuestaModel
    {
        /// <summary>
        /// Identificador único del reclamo.
        /// </summary>

        [JsonProperty("idReclamo")]
        public long Id { get; set; }

        /// <summary>
        /// Título del reclamo.
        /// </summary>

        [JsonProperty("titulo")]
        public string Titulo { get; set; } = string.Empty;

        /// <summary>
        /// Descripción detallada del reclamo.
        /// </summary>
        /// 
        [JsonProperty("descripcion")]
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>
        /// Identificador del estado actual del reclamo.
        /// </summary>
         
        [JsonProperty("idEstadoReclamo")]
        public int IdEstadoReclamo { get; set; }

        /// <summary>
        /// Descripción del estado actual del reclamo.
        /// </summary>
         
        [JsonProperty("descripcionEstadoReclamo")]
        public string DescripcionEstadoReclamo { get; set; } = string.Empty;

        /// <summary>
        /// Identificador del usuario externo que realizó el reclamo.
        /// </summary>
         
        [JsonProperty("idUsuario")]
        public long IdUsuarioExterno { get; set; }

        /// <summary>
        /// Nombre del usuario externo que realizó el reclamo.
        /// </summary>
        
        [JsonProperty("nombreUsuario")]
        public string NombreUsuarioExterno { get; set; } = string.Empty;

        /// <summary>
        /// Fecha en que se registró el reclamo.
        /// </summary>
         
        [JsonProperty("fechaIngreso")]
        public DateTime FechaRegistro { get; set; }

        /// <summary>
        /// Identificador del departamento actual encargado del reclamo.
        /// </summary>
         
        [JsonProperty("idDepartamentoActual")]
        public long IdDepartamentoActual { get; set; }

        /// <summary>
        /// Descripción del departamento actual encargado del reclamo.
        /// </summary>
        [JsonProperty("descripcionDepartamento")]
        public string DescripcionDepartamento { get; set; } = string.Empty;

        /// <summary>
        /// Identificador del estado detalle actual del reclamo. Solo se incluye en vistas de usuarios internos.
        /// </summary>
        [JsonProperty("idEstadoDetalleReclamo")]
        public int? IdEstadoDetalleReclamo { get; set; }

        /// <summary>
        /// Descripción del estado detalle actual del reclamo. Solo se incluye en vistas de usuarios internos.
        /// </summary>
        [JsonProperty("descripcionEstadoDetalleReclamo")]
        public string DescripcionEstadoDetalleReclamo { get; set; } = string.Empty;

        /// <summary>
        /// Indica si el reclamo es histórico.
        /// </summary>

        [JsonProperty("estaEnHistorico")]
        public bool EstaEnHistorico { get; set; }
    }
}
