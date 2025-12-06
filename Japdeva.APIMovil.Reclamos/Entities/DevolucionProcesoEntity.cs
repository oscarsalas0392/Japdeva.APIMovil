using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Japdeva.APIMovil.Reclamos.Entities
{
    /// <summary>
    /// Entidad que representa la configuración de devolución de procesos en el sistema de reclamos.
    /// Define las reglas y orden de devolución cuando un reclamo necesita regresar a una etapa anterior
    /// del flujo de procesamiento debido a rechazos o necesidad de información adicional.
    /// </summary>
    [Table("Tbl_DevolucionProceso")]
    public class DevolucionProcesoEntity
    {
        /// <summary>
        /// Obtiene o establece el identificador único de la configuración de devolución.
        /// </summary>
        [Column("id")]
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador de la orden de proceso desde la cual se origina la devolución.
        /// Define el paso del flujo desde el cual el reclamo será devuelto a una etapa anterior.
        /// </summary>
        [Column("idOrdenProceso")]
        public int IdOrdenProceso { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador de la orden de proceso hacia la cual se dirige la devolución.
        /// Especifica a qué etapa anterior del flujo debe regresar el reclamo.
        /// </summary>
        [Column("idOrdenDevolucionProceso")]
        public int OrdenDevolucionProceso { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del usuario interno que configuró esta regla de devolución.
        /// Permite auditoría de quién estableció las reglas de devolución en el sistema.
        /// </summary>
        [Column("idUsuarioInterno")]
        public long IdUsuarioInterno { get; set; }

        /// <summary>
        /// Obtiene o establece la fecha y hora de registro de la configuración de devolución.
        /// Timestamp de cuando se creó esta regla en el sistema.
        /// </summary>
        [Column("fechaRegistro")]
        public DateTime FechaRegistro { get; set; }

        /// <summary>
        /// Obtiene o establece el indicador de si la configuración de devolución está activa.
        /// Permite habilitar o deshabilitar reglas de devolución sin eliminarlas del sistema.
        /// </summary>
        [Column("activo")]
        public bool Activo { get; set; }
    }
}
