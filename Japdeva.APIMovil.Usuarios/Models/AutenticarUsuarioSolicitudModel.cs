namespace Japdeva.APIMovil.Usuarios.Models
{
    /// <summary>
    /// Modelo de solicitud para autenticar un usuario con sus credenciales.
    /// </summary>
    public class AutenticarUsuarioSolicitudModel
    {
        /// <summary>Obtiene o establece el correo electrónico del usuario.</summary>
        public string Correo { get; set; } = string.Empty;

        /// <summary>Obtiene o establece la contraseña del usuario.</summary>
        public string Contrasena { get; set; } = string.Empty;
    }
}
