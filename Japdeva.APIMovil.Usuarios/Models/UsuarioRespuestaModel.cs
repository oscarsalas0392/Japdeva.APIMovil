namespace Japdeva.APIMovil.Usuarios.Models
{
    /// <summary>
    /// Modelo de respuesta de usuario.
    /// </summary>
    public class UsuarioRespuestaModel
    {
        /// <summary>
        /// Obtiene o establece el identificador único del usuario.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Obtiene o establece el nombre del usuario.
        /// </summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece el correo electrónico del usuario.
        /// </summary>
        public string Correo { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece el teléfono del usuario.
        /// </summary>
        public string Telefono { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece la fecha de creación del usuario.
        /// </summary>
        public DateTime FechaCreacion { get; set; }

        /// <summary>
        /// Obtiene o establece la fecha de última actualización del usuario.
        /// </summary>
        public DateTime? FechaActualizacion { get; set; }

        /// <summary>
        /// Obtiene o establece un valor que indica si el usuario está activo.
        /// </summary>
        public bool Activo { get; set; }
    }
}
