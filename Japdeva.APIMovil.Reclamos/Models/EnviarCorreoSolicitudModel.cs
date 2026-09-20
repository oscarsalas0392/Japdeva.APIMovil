namespace Japdeva.APIMovil.Reclamos.Models
{
    /// <summary>
    /// Modelo con los datos necesarios para solicitar el envío de un correo a través de la cola EnviarCorreo.
    /// </summary>
    public class EnviarCorreoSolicitudModel
    {
        /// <summary>Dirección de correo electrónico del destinatario.</summary>
        public string Destinatario { get; set; } = string.Empty;

        /// <summary>Asunto del correo electrónico.</summary>
        public string Asunto { get; set; } = string.Empty;

        /// <summary>Cuerpo del correo electrónico.</summary>
        public string Cuerpo { get; set; } = string.Empty;

        /// <summary>Indica si el cuerpo del correo está en formato HTML.</summary>
        public bool EsCuerpoHtml { get; set; } = true;
    }
}
