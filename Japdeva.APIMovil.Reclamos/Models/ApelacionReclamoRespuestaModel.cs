using Newtonsoft.Json;

namespace Japdeva.APIMovil.Reclamos.Models
{
    /// <summary>
    /// Modelo de respuesta para consultas de apelaciones de reclamo.
    /// </summary>
    public class ApelacionReclamoRespuestaModel
    {
        /// <summary>Identificador único de la apelación.</summary>
        [JsonProperty("idApelacionReclamo")]
        public long Id { get; set; }

        /// <summary>Identificador del reclamo original sobre el cual se presentó la apelación.</summary>
        [JsonProperty("idReclamo")]
        public long IdReclamo { get; set; }

        /// <summary>Título de la apelación.</summary>
        [JsonProperty("titulo")]
        public string Titulo { get; set; } = string.Empty;

        /// <summary>Descripción detallada de la apelación.</summary>
        [JsonProperty("descripcion")]
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>Identificador del estado actual de la apelación.</summary>
        [JsonProperty("idEstadoReclamo")]
        public int IdEstadoReclamo { get; set; }

        /// <summary>Descripción del estado actual de la apelación.</summary>
        [JsonProperty("descripcionEstadoReclamo")]
        public string DescripcionEstadoReclamo { get; set; } = string.Empty;

        /// <summary>Identificador del usuario externo que presentó la apelación.</summary>
        [JsonProperty("idUsuario")]
        public long IdUsuarioExterno { get; set; }

        /// <summary>Fecha en que se registró la apelación.</summary>
        [JsonProperty("fechaIngreso")]
        public DateTime FechaRegistro { get; set; }

        /// <summary>Identificador del departamento que atiende actualmente la apelación.</summary>
        [JsonProperty("idDepartamentoActual")]
        public long IdDepartamentoActual { get; set; }

        /// <summary>Descripción del departamento actual encargado de la apelación.</summary>
        [JsonProperty("descripcionDepartamento")]
        public string DescripcionDepartamento { get; set; } = string.Empty;
    }
}
