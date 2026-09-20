namespace Japdeva.APIMovil.Usuarios.Models
{
    /// <summary>
    /// Modelo de solicitud para asignar un rol a un usuario.
    /// </summary>
    public class AgregarUsuarioRolSolicitudModel
    {
        /// <summary>Obtiene o establece el identificador del usuario.</summary>
        public int IdUsuario { get; set; }

        /// <summary>Obtiene o establece el identificador del rol a asignar.</summary>
        public int IdRol { get; set; }

        /// <summary>Obtiene o establece el identificador del administrador que realiza la asignación.</summary>
        public int IdUsuarioAdministrador { get; set; }
    }
}
