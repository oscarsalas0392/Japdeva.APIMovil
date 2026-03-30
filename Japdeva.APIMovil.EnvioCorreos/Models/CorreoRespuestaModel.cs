namespace Japdeva.APIMovil.EnvioCorreos.Models
{
    /// <summary>
    /// Modelo de respuesta con los datos de un correo en la cola de envío.
    /// </summary>
    public class CorreoRespuestaModel
    {
        /// <summary>Obtiene o establece el identificador único del registro.</summary>
        public long Id { get; set; }

        /// <summary>Obtiene o establece el correo electrónico del destinatario.</summary>
        public string Destinatario { get; set; } = string.Empty;

        /// <summary>Obtiene o establece el asunto del correo.</summary>
        public string Asunto { get; set; } = string.Empty;

        /// <summary>Obtiene o establece el número de intentos realizados.</summary>
        public int Intentos { get; set; }

        /// <summary>Obtiene o establece si el correo fue enviado exitosamente.</summary>
        public bool Enviado { get; set; }

        /// <summary>Obtiene o establece la fecha de registro.</summary>
        public DateTime FechaRegistro { get; set; }
    }
}
