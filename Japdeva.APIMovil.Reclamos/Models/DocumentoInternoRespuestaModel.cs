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
    }
}
