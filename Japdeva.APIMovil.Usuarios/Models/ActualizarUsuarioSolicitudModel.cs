namespace Japdeva.APIMovil.Usuarios.Models
{
    /// <summary>
    /// Modelo de solicitud para actualizar los datos de un usuario existente.
    /// </summary>
    public class ActualizarUsuarioSolicitudModel
    {
        /// <summary>Obtiene o establece el identificador del usuario a actualizar.</summary>
        public int Id { get; set; }

        /// <summary>Obtiene o establece el nombre del usuario.</summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>Obtiene o establece los apellidos del usuario.</summary>
        public string Apellidos { get; set; } = string.Empty;

    }
}
