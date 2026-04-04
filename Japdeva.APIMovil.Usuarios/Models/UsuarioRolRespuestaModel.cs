namespace Japdeva.APIMovil.Usuarios.Models
{
    /// <summary>
    /// Modelo de respuesta para la asignación de un rol a un usuario.
    /// </summary>
    public class UsuarioRolRespuestaModel
    {
        /// <summary>Obtiene o establece el identificador único del registro.</summary>
        public long Id { get; set; }

        /// <summary>Obtiene o establece el identificador del usuario.</summary>
        public int IdUsuario { get; set; }

        /// <summary>Obtiene o establece el identificador del rol asignado.</summary>
        public int IdRol { get; set; }


        /// <summary>Obtiene o establece la fecha de creación del registro.</summary>
        public DateTime FechaRegistro { get; set; }

        /// <summary>Obtiene o establece si la asignación está activa.</summary>
        public bool Activo { get; set; }
    }
}
