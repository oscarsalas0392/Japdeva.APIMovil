namespace Japdeva.APIMovil.Usuarios.Models
{
    /// <summary>
    /// Modelo de respuesta para un departamento de la organización.
    /// </summary>
    public class DepartamentoRespuestaModel
    {
        /// <summary>Obtiene o establece el identificador único del departamento.</summary>
        public int Id { get; set; }

        /// <summary>Obtiene o establece la descripción del departamento.</summary>
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>Obtiene o establece si el departamento está activo.</summary>
        public bool Activo { get; set; }
    }
}
