namespace Japdeva.APIMovil.Common.Models
{
    /// <summary>
    /// Modelo que representa un mensaje recibido desde el microservicio de Colas.
    /// </summary>
    public class ColaMensajeModel
    {
        /// <summary>Identificador único del mensaje en base de datos.</summary>
        public long Id { get; set; }

        /// <summary>Identificador RPC asignado por el microservicio productor para correlacionar la respuesta.</summary>
        public string IdRpc { get; set; } = string.Empty;

        /// <summary>Identificador de la cola a la que pertenece el mensaje.</summary>
        public long ColaId { get; set; }

        /// <summary>Identificador de trazabilidad distribuida.</summary>
        public string TraceId { get; set; } = string.Empty;

        /// <summary>Contenido del mensaje en formato JSON.</summary>
        public string Contenido { get; set; } = string.Empty;

        /// <summary>Estado actual del mensaje (1=Pendiente, 2=EnProceso, 3=Exitoso, 4=Fallido, 5=Expirado).</summary>
        public int Estado { get; set; }

        /// <summary>Metadatos adicionales del mensaje.</summary>
        public string MetaDatos { get; set; } = string.Empty;

        /// <summary>Indica si el TraceId del mensaje difiere del TraceId de la solicitud original.</summary>
        public bool TraceIdDiferente { get; set; }
    }
}
