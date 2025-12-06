using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Japdeva.APIMovil.Reclamos.Entities
{
    /// <summary>
    /// Entidad que representa los posibles estados de los detalles de reclamo en el sistema.
    /// Define el catálogo de estados por los que puede pasar un detalle durante el proceso de atención del reclamo.
    /// </summary>
    [Table ("Tbl_EstadoDetalleReclamo")]
    public class EstadoDetalleReclamoEntity
    {
        /// <summary>
        /// Obtiene o establece el identificador único del estado de detalle de reclamo.
        /// </summary>
        [Column("id")]
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Obtiene o establece la descripción del estado de detalle de reclamo.
        /// Nombre descriptivo del estado como Pendiente, En Revisión, Completado, Rechazado.
        /// </summary>
        [Column("descripcion")]
        [MaxLength(100)]
        [MinLength(1)]
        [Required]
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece el indicador de si el estado permite continuar con el proceso.
        /// Define si al establecer este estado el reclamo puede avanzar al siguiente paso.
        /// </summary>
        [Column("continuaProceso")]
        public bool ContinuaProceso { get; set; }

        /// <summary>
        /// Obtiene o establece el indicador de si el estado rechaza el proceso.
        /// Define si al establecer este estado el reclamo es rechazado o no puede continuar.
        /// </summary>
        [Column("rechazaProceso")]
        public bool RechazaProceso { get; set; }

        /// <summary>
        /// Obtiene o establece el indicador de si el estado implica devolución del proceso.
        /// Define si al establecer este estado el reclamo debe ser devuelto a una etapa anterior.
        /// </summary>
        [Column("devolucionProceso")]
        public bool DevolucionProceso { get; set; }

        /// <summary>
        /// Obtiene o establece el indicador de si el estado finaliza el proceso.
        /// Define si al establecer este estado el reclamo se considera completamente procesado.
        /// </summary>
        [Column("finalizarProceso")]
        public bool FinalizarProceso { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del usuario interno que registró el estado.
        /// </summary>
        [Column("idUsuarioInterno")]
        public long IdUsuarioInterno { get; set; }

        /// <summary>
        /// Obtiene o establece la fecha y hora de registro del estado.
        /// </summary>
        [Column("fechaRegistro")]
        public DateTime FechaRegistro { get; set; }

        /// <summary>
        /// Obtiene o establece el indicador de si el estado está activo y disponible para uso.
        /// </summary>
        [Column("activo")]
        public bool Activo { get; set; }

    }
}
