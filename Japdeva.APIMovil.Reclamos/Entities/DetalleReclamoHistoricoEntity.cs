using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Japdeva.APIMovil.Reclamos.Entities
{
    /// <summary>
    /// Entidad que representa el histórico de detalles de reclamos en el sistema.
    /// Almacena un registro de auditoría de todos los cambios y actualizaciones realizados
    /// a los detalles de reclamos, proporcionando trazabilidad completa del proceso.
    /// </summary>
    [Table("Tbl_DetalleReclamoHistorico")]
    public class DetalleReclamoHistoricoEntity
    {
        /// <summary>
        /// Obtiene o establece el identificador único del registro histórico.
        /// </summary>
        [Key]
        [Column("id")]
        public long Id { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del reclamo al cual pertenece este detalle histórico.
        /// Referencia al reclamo principal para mantener la relación con el registro original.
        /// </summary>
        [Column("idReclamo")]
        public long IdReclamo { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del orden de proceso asociado al detalle histórico.
        /// Define el flujo o secuencia del proceso en el momento que se registró el histórico.
        /// </summary>
        [Column("idOrdenProceso")]
        public int IdOrdenProceso { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del proceso de devolución, si aplica.
        /// Referencia opcional para casos donde el detalle requirió una devolución en el proceso.
        /// </summary>
        [Column("idDevolucionProceso")]
        public int? IdDevolucionProceso { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del estado del detalle de reclamo en el momento del registro.
        /// Captura el estado específico que tenía el detalle cuando se creó este registro histórico.
        /// </summary>
        [Column("idEstadoDetalleReclamo")]
        public int IdEstadoDetalleReclamo { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del usuario interno que realizó la acción registrada.
        /// Permite auditoría de quién ejecutó cada cambio en el detalle del reclamo.
        /// </summary>
        [Column("idUsuarioInterno")]
        public long IdUsuarioInterno { get; set; }

        /// <summary>
        /// Obtiene o establece la descripción del cambio o acción realizada en el detalle.
        /// Comentarios, observaciones o detalles específicos sobre la modificación registrada.
        /// </summary>
        [Column("descripcion")]
        [MaxLength(500)]
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece la fecha y hora en que se registró este cambio histórico.
        /// Timestamp exacto de cuando ocurrió la modificación para auditoría temporal.
        /// </summary>
        [Column("fechaRegistro")]
        public DateTime FechaRegistro { get; set; }
    }
}
