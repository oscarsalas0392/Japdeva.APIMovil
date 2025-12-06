using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Japdeva.APIMovil.Reclamos.Entities
{
    /// <summary>
    /// Entidad que representa la relación entre estados de detalle de reclamo y órdenes de proceso.
    /// Define qué estados están disponibles y son válidos para cada paso específico del flujo
    /// de procesamiento de reclamos, estableciendo las reglas del workflow del sistema.
    /// </summary>
    [Table("Tbl_EstadoDetalleReclamoOrdenProceso")]
    public class EstadoDetalleReclamoOrdenProcesoEntity
    {
        /// <summary>
        /// Obtiene o establece el identificador único de la relación estado-orden.
        /// </summary>
        [Column("id")]
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador de la orden de proceso en la relación.
        /// Define en qué paso del flujo de trabajo aplica esta configuración de estado.
        /// </summary>
        [Column("idOrdenProceso")]
        public int IdOrdenProceso { get; set; } 

        /// <summary>
        /// Obtiene o establece el identificador del estado de detalle de reclamo en la relación.
        /// Especifica qué estado puede ser utilizado en la orden de proceso correspondiente.
        /// </summary>
        [Column("idEstadoDetalleReclamo")]
        public int IdEstadoDetalleReclamo { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del usuario interno que configuró esta relación.
        /// Permite auditoría de quién estableció las reglas de workflow en el sistema.
        /// </summary>
        [Column("idUsuarioInterno")]
        public long IdUsuarioInterno { get; set; }

        /// <summary>
        /// Obtiene o establece la fecha y hora de registro de la relación.
        /// Timestamp de cuando se configuró esta regla de workflow.
        /// </summary>
        [Column("fechaRegistro")]
        public DateTime FechaRegistro { get; set; }

        /// <summary>
        /// Obtiene o establece el indicador de si la relación está activa.
        /// Permite habilitar o deshabilitar reglas de workflow sin eliminarlas del sistema.
        /// </summary>
        [Column("activo")]
        public bool Activo { get; set; }
    }
}
