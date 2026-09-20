namespace Japdeva.APIMovil.Usuarios.Models
{
    /// <summary>
    /// Modelo de solicitud para actualizar la contraseña de un usuario.
    /// </summary>
    public class ActualizarContrasenaUsuarioSolicitudModel
    {
        /// <summary>
        /// Obtiene o establece el identificador del usuario.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Obtiene o establece la contraseña actual del usuario.
        /// </summary>
        public string ContrasenaAnterior { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece la nueva contraseña del usuario.
        /// </summary>
        public string ContrasenaNueva { get; set; } = string.Empty;
    }
}
