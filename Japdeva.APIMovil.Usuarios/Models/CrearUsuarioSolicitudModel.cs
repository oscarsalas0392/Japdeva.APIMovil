using Newtonsoft.Json;

namespace Japdeva.APIMovil.Usuarios.Models
{
    /// <summary>
    /// Modelo de solicitud para crear un usuario.
    /// </summary>
    public class CrearUsuarioSolicitudModel
    {
        /// <summary>
        /// Obtiene o establece el nombre del usuario.
        /// </summary>
        /// 
        [JsonProperty("nombre")]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece los apellidos del usuario.
        /// </summary>
        /// 
        [JsonProperty("apellidos")]
        public string Apellidos { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece la identificacion del usuario.
        /// </summary>
        /// 
        [JsonProperty("identificacion")]
        public string Identificacion { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece el correo electrónico del usuario.
        /// </summary>
        /// 
        [JsonProperty("correo")]

        public string Correo { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece la contraseña del usuario.
        /// </summary>
        /// 
        [JsonProperty("contrasena")]
        public string Contrasena { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece el teléfono del usuario.
        /// </summary>
        /// 
        [JsonProperty("telefono")]

        public string Telefono { get; set; } = string.Empty;       
    }
}
