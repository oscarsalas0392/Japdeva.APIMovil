namespace Japdeva.APIMovil.Usuarios.Models
{
    /// <summary>
    /// Modelo de respuesta con los datos básicos del usuario autenticado.
    /// </summary>
    public class AutenticarUsuarioRespuestaModel
    {
        /// <summary>Obtiene o establece el identificador del usuario.</summary>
        public int Id { get; set; }

        /// <summary>Obtiene o establece el nombre del usuario.</summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>Obtiene o establece los apellidos del usuario.</summary>
        public string Apellidos { get; set; } = string.Empty;

        /// <summary>Obtiene o establece el correo electrónico del usuario.</summary>
        public string Correo { get; set; } = string.Empty;

        /// <summary>Obtiene o establece el número de identificación del usuario.</summary>
        public string Identificacion { get; set; } = string.Empty;

        /// <summary>Obtiene o establece el número de teléfono del usuario.</summary>
        public string? Telefono { get; set; }

        /// <summary>Obtiene o establece la fecha de nacimiento del usuario.</summary>
        public DateTime? FechaNacimiento { get; set; }
    }
}
