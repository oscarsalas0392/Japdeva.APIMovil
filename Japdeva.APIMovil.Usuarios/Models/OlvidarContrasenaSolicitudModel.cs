namespace Japdeva.APIMovil.Usuarios.Models
{
    /// <summary>
    /// Modelo de solicitud para el proceso de recuperación de contraseña.
    /// </summary>
    public class OlvidarContrasenaSolicitudModel
    {
        /// <summary>
        /// Obtiene o establece el correo electrónico del usuario que olvidó su contraseña.
        /// </summary>
        public string Correo { get; set; } = string.Empty;
    }
}
