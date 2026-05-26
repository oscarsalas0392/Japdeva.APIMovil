using Newtonsoft.Json;

namespace Japdeva.APIMovil.Reclamos.Models
{
    /// <summary>
    /// Representa la respuesta de un documento interno asociado a un reclamo.
    /// </summary>
    public class DocumentoInternoRespuestaModel
    {
        /// <summary>
        /// Obtiene o establece el identificador único del documento interno.
        /// </summary>
        [JsonProperty("idDocumento")]
        public long Id { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del detalle del reclamo asociado al documento interno.
        /// </summary>
        [JsonProperty("idDetalleReclamo")]
        public long IdDetalleReclamo { get; set; }

        /// <summary>
        /// Obtiene o establece el nombre del documento interno.
        /// </summary>
        [JsonProperty("nombreArchivo")]
        public string NombreDocumento { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece el contenido del documento interno en formato base64.
        /// </summary>
        [JsonProperty("archivo")]
        public string Documento { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece la descripción del detalle de reclamo al que pertenece el documento.
        /// </summary>
        [JsonProperty("descripcionDetalleReclamo")]
        public string DescripcionDetalleReclamo { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece el nombre del departamento asociado al detalle de reclamo.
        /// </summary>
        [JsonProperty("nombreDepartamento")]
        public string NombreDepartamento { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece la fecha de inicio del detalle de reclamo.
        /// </summary>
        [JsonProperty("fechaInicio")]
        public DateTime FechaInicio { get; set; }

        /// <summary>
        /// Obtiene o establece la fecha de fin del detalle de reclamo.
        /// </summary>
        [JsonProperty("fechaFin")]
        public DateTime? FechaFin { get; set; }

        /// <summary>
        /// Obtiene o establece el nombre del usuario interno que atendió el detalle.
        /// </summary>
        [JsonProperty("nombreUsuarioInterno")]
        public string NombreUsuarioInterno { get; set; } = string.Empty;
    }
}
