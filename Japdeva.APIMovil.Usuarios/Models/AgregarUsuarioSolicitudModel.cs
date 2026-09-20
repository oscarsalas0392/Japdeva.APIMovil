namespace Japdeva.APIMovil.Usuarios.Models
{
    /// <summary>
    /// Modelo de solicitud para registrar un nuevo usuario en el sistema.
    /// </summary>
    public class AgregarUsuarioSolicitudModel
    {
        /// <summary>Obtiene o establece el número de identificación (cédula) del usuario.</summary>
        public string Identificacion { get; set; } = string.Empty;

        /// <summary>Obtiene o establece el identificador del tipo de cédula.</summary>
        public int IdTipoCedula { get; set; }

        /// <summary>Obtiene o establece el nombre del usuario.</summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>Obtiene o establece los apellidos del usuario.</summary>
        public string Apellidos { get; set; } = string.Empty;

        /// <summary>Obtiene o establece el correo electrónico del usuario.</summary>
        public string Correo { get; set; } = string.Empty;

        /// <summary>Obtiene o establece la contraseña del usuario.</summary>
        public string Contrasena { get; set; } = string.Empty;

        /// <summary>Obtiene o establece el número de teléfono del usuario.</summary>
        public string? Telefono { get; set; }

        /// <summary>Obtiene o establece la fecha de nacimiento del usuario.</summary>
        public DateTime? FechaNacimiento { get; set; }
    }
}
