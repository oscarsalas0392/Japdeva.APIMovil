namespace Japdeva.APIMovil.Reclamos.Models
{
    /// <summary>
    /// Modelo que representa la respuesta con la plantilla de correo obtenida desde la cola ObtenerPlantilla.
    /// </summary>
    public class PlantillaRespuestaModel
    {
        /// <summary>Contenido HTML de la plantilla de correo.</summary>
        public string Plantilla { get; set; } = string.Empty;

        /// <summary>Indica si la plantilla está en formato HTML.</summary>
        public bool EsHtml { get; set; }

        /// <summary>Asunto del correo asociado a la plantilla.</summary>
        public string Asunto { get; set; } = string.Empty;
    }
}
