using System.Text.Json.Serialization;

namespace Japdeva.APIMovil.Reclamos.Models
{
    /// <summary>
    /// Modelo de respuesta para la operación de creación de un reclamo.
    /// Contiene la información del reclamo creado exitosamente en el sistema.
    /// </summary>
    public class AgregarReclamoRespuestaModel
    {
        /// <summary>
        /// Obtiene o establece el identificador único del reclamo creado.
        /// Identificador generado por el sistema al registrar el reclamo.
        /// </summary>
        [JsonPropertyName("idReclamo")]
        public long Id { get; set; }

        /// <summary>
        /// Obtiene o establece el título del reclamo creado.
        /// Confirmación del título que fue registrado en el sistema.
        /// </summary>
        [JsonPropertyName("titulo")]
        public string Titulo { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece la descripción del reclamo creado.
        /// Confirmación de la descripción que fue registrada en el sistema.
        /// </summary>
        [JsonPropertyName("descripcion")]
        public string Descripcion { get; set; } = string.Empty;
    }
}
