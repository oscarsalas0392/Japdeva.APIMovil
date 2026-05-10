namespace Japdeva.APIMovil.Usuarios.Models
{
    /// <summary>
    /// Modelo de respuesta para la asignación de un usuario a un departamento.
    /// </summary>
    public class DepartamentoUsuarioRespuestaModel
    {
        /// <summary>Obtiene o establece el identificador único del registro.</summary>
        public long Id { get; set; }

        /// <summary>Obtiene o establece el identificador del usuario asignado.</summary>
        public int IdUsuario { get; set; }

        /// <summary>Obtiene o establece el identificador del departamento.</summary>
        public int IdDepartamento { get; set; }

        /// <summary>Obtiene o establece la descripción del departamento asignado.</summary>
        public string DescripcionDepartamento { get; set; } = string.Empty;

        /// <summary>Obtiene o establece el identificador del administrador que realizó la asignación.</summary>
        public int IdUsuarioAdministrador { get; set; }

        /// <summary>Obtiene o establece la fecha de creación del registro.</summary>
        public DateTime FechaRegistro { get; set; }

        /// <summary>Obtiene o establece si la asignación está activa.</summary>
        public bool Activo { get; set; }
    }
}
