namespace Japdeva.APIMovil.Usuarios.Models
{
    /// <summary>
    /// Modelo de respuesta para un tipo de cédula.
    /// </summary>
    public class TipoCedulaRespuestaModel
    {
        /// <summary>Obtiene o establece el identificador único del tipo de cédula.</summary>
        public int Id { get; set; }

        /// <summary>Obtiene o establece el tipo de cédula.</summary>
        public string Tipo { get; set; } = string.Empty;

        /// <summary>Obtiene o establece el formato de validación.</summary>
        public string Formato { get; set; } = string.Empty;

        /// <summary>Obtiene o establece si el tipo de cédula está activo.</summary>
        public bool Activo { get; set; }
    }
}
