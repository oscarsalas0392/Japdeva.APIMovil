using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Japdeva.APIMovil.Reclamos.Entities
{
    /// <summary>
    /// Entidad que representa un detalle o seguimiento de un reclamo.
    /// Almacena información adicional, comentarios, actualizaciones de estado y seguimiento del proceso del reclamo.
    /// </summary>
    [Table("Tbl_DetalleReclamo")]
    public class DetalleReclamoEntity
    {
        /// <summary>
        /// Obtiene o establece el identificador único del detalle de reclamo.
        /// </summary>
        [Column("id")]
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del reclamo al cual pertenece este detalle.
        /// </summary>
        [Column("idReclamo")]
        public long IdReclamo { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del orden de proceso asociado al detalle.
        /// Define el flujo o secuencia del proceso de atención del reclamo.
        /// </summary>
        [Column("idOrdenProceso")]
        public int IdOrdenProceso { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador de la devolución asociada al detalle.
        /// Puede ser nulo si no aplica devolución.
        /// </summary>
        [Column("idDevolucionProceso")]
        public int? IdDevolucionProceso { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del estado del detalle de reclamo.
        /// Define el estado actual de este detalle específico dentro del proceso.
        /// </summary>
        [Column("idEstadoDetalleReclamo")]
        public int IdEstadoDetalleReclamo { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del usuario interno que registró el detalle.
        /// </summary>
        [Column("idUsuarioInterno")]
        public long? IdUsuarioInterno { get; set; }

        /// <summary>
        /// Obtiene o establece la descripción o comentario del detalle.
        /// Contiene información adicional sobre el estado, acciones realizadas o observaciones.
        /// </summary>
        [Column("descripcion")]
        [MaxLength(1000)]
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece la fecha y hora de registro del detalle.
        /// </summary>
        [Column("fechaRegistro")]
        public DateTime FechaRegistro { get; set; }

        /// <summary>
        /// Obtiene o establece la fecha y hora de la última edición del detalle.
        /// </summary>
        [Column("fechaEdicion")]
        public DateTime? FechaEdicion { get; set; }

    }
}
