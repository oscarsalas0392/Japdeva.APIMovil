using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Japdeva.APIMovil.Colas.Models;

namespace Japdeva.APIMovil.Colas.Entities
{
    /// <summary>
    /// Entidad que representa un mensaje en cola para persistencia en base de datos.
    /// </summary>
    [Table("Tbl_MensajeCola")]
    public class MensajeColaEntity
    {
        /// <summary>
        /// Obtiene o establece el identificador único del mensaje en cola.
        /// </summary>
        [Key]
        [Column("id")]
        public long Id { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador único de la llamada RPC.
        /// </summary>
        [Column("idRpc")]
        [MaxLength(100)]
        public string IdRpc { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece el nombre de la cola de destino.
        /// </summary>
        [Required]
        [MaxLength(100)]
        [Column("colaId")]
        public long ColaId { get; set; }

        /// <summary>
        /// Obtiene o establece el contenido del mensaje en formato JSON.
        /// </summary>
        [Required]
        [Column("contenidoMensaje")]
        public string ContenidoMensaje { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece el identificador del estado del mensaje.
        /// </summary>
        [Required]
        [Column("estadoId")]
        public int EstadoId { get; set; } = (int)EstadoMensajeModel.Pendiente;

        /// <summary>
        /// Obtiene o establece la prioridad del mensaje (1=Alta, 2=Media, 3=Baja).
        /// </summary>
        [Column("prioridadId")]
        public int PrioridadId { get; set; } = 2;

        /// <summary>
        /// Obtiene o establece el número de intentos de procesamiento.
        /// </summary>
        [Column("contadorReintentos")]
        public int ContadorReintentos { get; set; } = 0;


        /// <summary>
        /// Obtiene o establece la fecha y hora de creación del mensaje.
        /// </summary>
        [Column("fechaRegistro")]
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Obtiene o establece la fecha y hora de la última actualización.
        /// </summary>
        [Column("fechaEdicion")]
        public DateTime? FechaEdicion { get; set; } 


        /// <summary>
        /// Obtiene o establece el mensaje de error en caso de fallo.
        /// </summary>
        [Column("mensajeError")]
        public string MensajeError { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece el identificador de trazabilidad para seguimiento.
        /// </summary>
        [MaxLength(200)]
        [Column("traceId")]
        public string TraceId { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece metadatos adicionales en formato JSON.
        /// </summary>
        [Column("metadatos")]
        public string Metadatos { get; set; } = string.Empty;
    }
}