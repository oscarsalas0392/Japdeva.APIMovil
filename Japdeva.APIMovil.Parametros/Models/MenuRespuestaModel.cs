using Newtonsoft.Json;

namespace Japdeva.APIMovil.Parametros.Models
{
    /// <summary>
    /// Modelo de respuesta que representa un menú del sistema.
    /// Contiene información sobre el menú y su configuración de visualización.
    /// </summary>
    public class MenuRespuestaModel
    {
        /// <summary>
        /// Obtiene o establece el identificador único del menú.
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        /// Obtiene o establece el nombre del menú.
        /// </summary>
        [JsonProperty("nombre")]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece la ruta de navegación del menú.
        /// </summary>
        [JsonProperty("ruta")]
        public string? Ruta { get; set; }

        /// <summary>
        /// Obtiene o establece el nombre del ícono asociado al menú.
        /// </summary>
        [JsonProperty("icono")]
        public string? Icono { get; set; }

        /// <summary>
        /// Obtiene o establece el orden de visualización del menú.
        /// </summary>
        [JsonProperty("orden")]
        public int Orden { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del menú padre, si es un submenú.
        /// </summary>
        [JsonProperty("idPadre")]
        public int? IdPadre { get; set; }

        /// <summary>
        /// Obtiene o establece si el menú debe mostrarse en la interfaz.
        /// </summary>
        [JsonProperty("mostrar")]
        public bool Mostrar { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del usuario interno asociado al menú.
        /// </summary>
        [JsonProperty("idUsuarioInterno")]
        public long? IdUsuarioInterno { get; set; }
    }
}
