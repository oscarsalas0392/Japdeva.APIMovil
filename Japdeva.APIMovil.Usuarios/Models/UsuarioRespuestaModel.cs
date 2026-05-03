namespace Japdeva.APIMovil.Usuarios.Models
{
    /// <summary>
    /// Modelo de respuesta con los datos de un usuario del sistema.
    /// </summary>
    public class UsuarioRespuestaModel
    {
        /// <summary>Obtiene o establece el identificador único del usuario.</summary>
        public int Id { get; set; }

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

        /// <summary>Obtiene o establece la fecha de creación del registro.</summary>
        public DateTime FechaRegistro { get; set; }

        /// <summary>Obtiene o establece la fecha de última modificación.</summary>
        public DateTime? FechaEdicion { get; set; }

        /// <summary>Obtiene o establece el número de teléfono del usuario.</summary>
        public string? Telefono { get; set; }

        /// <summary>Obtiene o establece la fecha de nacimiento del usuario.</summary>
        public DateTime? FechaNacimiento { get; set; }

        /// <summary>Obtiene o establece si el usuario está activo.</summary>
        public bool Activo { get; set; }
    }
}
