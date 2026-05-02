using System.Text.Json.Serialization;

namespace Japdeva.APIMovil.Reclamos.Models
{
    /// <summary>
    /// Modelo de respuesta para la operación de creación de una apelación de reclamo.
    /// Contiene la información de la apelación registrada exitosamente en el sistema.
    /// </summary>
    public class AgregarApelacionReclamoRespuestaModel
    {
        /// <summary>
        /// Obtiene o establece el identificador único de la apelación creada.
        /// </summary>
        [JsonPropertyName("idApelacionReclamo")]
        public long Id { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del reclamo asociado a la apelación.
        /// </summary>
        [JsonPropertyName("idReclamo")]
        public long IdReclamo { get; set; }

        /// <summary>
        /// Obtiene o establece el título de la apelación creada.
        /// </summary>
        [JsonPropertyName("titulo")]
        public string Titulo { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece la descripción de la apelación creada.
        /// </summary>
        [JsonPropertyName("descripcion")]
        public string Descripcion { get; set; } = string.Empty;
    }
}
