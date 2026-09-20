namespace Japdeva.APIMovil.Usuarios.Models
{
    /// <summary>
    /// Modelo de solicitud para la actualización de la asignación de un rol a un usuario.
    /// </summary>
    public class ActualizarUsuarioRolSolicitudModel
    {
        /// <summary>
        /// Obtiene o establece el identificador de la asignación a actualizar.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Obtiene o establece el nuevo identificador del rol.
        /// </summary>
        public int IdRol { get; set; }

        /// <summary>
        /// Obtiene o establece el nuevo identificador del usuario
        /// </summary>
        public int IdUsuario { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del usuario administrador que realiza la actualización.
        /// </summary>
        public int? IdUsuarioAdministrador { get; set; }
    }
}
