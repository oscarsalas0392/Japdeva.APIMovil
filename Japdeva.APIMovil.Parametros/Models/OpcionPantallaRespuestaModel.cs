using Newtonsoft.Json;

namespace Japdeva.APIMovil.Parametros.Models
{
    /// <summary>
    /// Modelo de respuesta que representa una opción de pantalla del sistema.
    /// </summary>
    public class OpcionPantallaRespuestaModel
    {
        /// <summary>
        /// Obtiene o establece el identificador único de la opción de pantalla.
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        /// Obtiene o establece el nombre clave de la opción de pantalla.
        /// </summary>
        [JsonProperty("nombre")]
        public string Nombre { get; set; } = string.Empty;
    }
}
