using Newtonsoft.Json;

namespace Japdeva.APIMovil.Reclamos.Models
{
    /// <summary>
    /// Representa la respuesta de un documento asociado a un usuario, incluyendo su identificador, 
    /// el reclamo relacionado, el nombre del archivo y el contenido en formato base64.
    /// </summary>
    public class DocumentoUsuarioRespuestaModel
    {
        /// <summary>
        /// Obtiene o establece el identificador único del documento interno.
        /// </summary>
        [JsonProperty("idDocumento")]
        public long Id { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del reclamo asociado al documento interno.
        /// </summary>
        [JsonProperty("idReclamo")]
        public long IdReclamo { get; set; }

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
