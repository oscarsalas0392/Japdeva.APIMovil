using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Japdeva.APIMovil.Colas.Entities
{
    /// <summary>
    /// Entidad que representa el estado de una cola en el sistema.
    /// </summary>
    [Table("queue_status")]
    public class EstadoColaEntity
    {
        /// <summary>
        /// Obtiene o establece el identificador único del estado de cola.
        /// </summary>
        [Key]
        [Column("id")]
        public Int64 Id { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador de la cola.
        /// </summary>
        [Required]
        [MaxLength(100)]
        [Column("cola")]
        public Int64 ColaId { get; set; }

        /// <summary>
        /// Obtiene o establece el número total de mensajes en la cola.
        /// </summary>
        [Column("mensajesTotales")]
        public int TotalMensajes { get; set; } = 0;

        /// <summary>
        /// Obtiene o establece el número de mensajes pendientes de procesamiento.
        /// </summary>
        [Column("mensajesPendientes")]
        public int MensajesPendientes { get; set; } = 0;

        /// <summary>
        /// Obtiene o establece el número de mensajes procesados exitosamente.
        /// </summary>
        [Column("mensajesProcesados")]
        public int MensajesProcesados { get; set; } = 0;

        /// <summary>
        /// Obtiene o establece el número de mensajes fallidos.
        /// </summary>
        [Column("mensajesFallidos")]
        public int MensajesFallidos { get; set; } = 0;

        /// <summary>
        /// Obtiene o establece la fecha y hora de la última actualización.
        /// </summary>
        [Column("fechaEdicion")]
        public DateTime fechaEdicion { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Obtiene o establece la fecha y hora del último procesamiento.
        /// </summary>
        [Column("fechaRegistro")]
        public DateTime? fechaRegistro { get; set; }

    }
}