namespace Japdeva.APIMovil.Reclamos.Models
{
    /// <summary>
    /// Modelo que representa los datos básicos de un usuario obtenidos desde la cola ObtenerUsuario.
    /// </summary>
    public class UsuarioDatosRespuestaModel
    {
        /// <summary>Nombre del usuario.</summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>Apellidos del usuario.</summary>
        public string Apellidos { get; set; } = string.Empty;

        /// <summary>Correo electrónico del usuario.</summary>
        public string Correo { get; set; } = string.Empty;
    }
}
