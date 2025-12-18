namespace Japdeva.APIMovil.Usuarios.Models
{
    /// <summary>
    /// Modelo de solicitud para actualizar un usuario.
    /// </summary>
    public class ActualizarUsuarioSolicitudModel
    {
        /// <summary>
        /// Obtiene o establece el identificador del usuario.
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
        /// Obtiene o establece un valor que indica si el usuario está activo.
        /// </summary>
        public bool Activo { get; set; }
    }
}
