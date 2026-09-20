namespace Japdeva.APIMovil.Usuarios.Models
{
    /// <summary>
    /// Modelo de respuesta para un rol del sistema.
    /// </summary>
    public class RolRespuestaModel
    {
        /// <summary>Obtiene o establece el identificador único del rol.</summary>
        public int Id { get; set; }

        /// <summary>Obtiene o establece la descripción del rol.</summary>
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>Obtiene o establece si el rol está activo.</summary>
        public bool Activo { get; set; }
    }
}
