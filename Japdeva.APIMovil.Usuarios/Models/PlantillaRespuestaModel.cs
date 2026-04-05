namespace Japdeva.APIMovil.Usuarios.Models
{
    /// <summary>
    /// Modelo que representa la respuesta con la plantilla de correo obtenida desde la cola.
    /// </summary>
    public class PlantillaRespuestaModel
    {
        /// <summary>Contenido HTML de la plantilla de correo.</summary>
        public string Plantilla { get; set; } = string.Empty;
    }
}
