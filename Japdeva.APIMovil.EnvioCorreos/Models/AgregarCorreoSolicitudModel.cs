namespace Japdeva.APIMovil.EnvioCorreos.Models
{
    /// <summary>
    /// Modelo de solicitud para registrar un nuevo correo en la cola de envío.
    /// </summary>
    public class AgregarCorreoSolicitudModel
    {
        /// <summary>Obtiene o establece el correo electrónico del destinatario.</summary>
        public string Destinatario { get; set; } = string.Empty;

        /// <summary>Obtiene o establece el asunto del correo.</summary>
        public string Asunto { get; set; } = string.Empty;

        /// <summary>Obtiene o establece el cuerpo del correo.</summary>
        public string Cuerpo { get; set; } = string.Empty;

        /// <summary>Obtiene o establece si el cuerpo del correo está en formato HTML. Por defecto true.</summary>
        public bool EsCuerpoHtml { get; set; } = true;
    }
}
