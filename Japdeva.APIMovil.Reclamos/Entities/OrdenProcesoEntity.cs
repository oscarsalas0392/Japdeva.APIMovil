using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Japdeva.APIMovil.Reclamos.Entities
{
    /// <summary>
    /// Entidad que representa una orden o paso en el proceso de atención de reclamos.
    /// Define la secuencia y configuración de cada etapa del workflow de procesamiento,
    /// estableciendo el flujo ordenado que deben seguir los reclamos desde su recepción hasta su resolución.
    /// </summary>
    [Table("Tbl_OrdenProceso")]
    public class OrdenProcesoEntity
    {
        /// <summary>
        /// Obtiene o establece el identificador único de la orden de proceso.
        /// </summary>
        [Column("id")]
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Obtiene o establece la descripción de la orden de proceso.
        /// Nombre descriptivo del paso o etapa en el workflow como Validación Inicial, Análisis Técnico, Resolución.
        /// </summary>
        [Column("descripcion")]
        [MaxLength(100)]
        [MinLength(1)]
        [Required]
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece el número de orden o secuencia de este paso en el proceso.
        /// Define la posición de esta etapa dentro del flujo completo de procesamiento de reclamos.
        /// </summary>
        [Column("orden")]
        public int Orden { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del usuario interno que configuró esta orden de proceso.
        /// Permite auditoría de quién estableció la configuración del workflow.
        /// </summary>
        [Column("idUsuarioInterno")]
        public long IdUsuarioInterno { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del departamento.
        /// Permite identificar cual departamento debe continuar con el proceso del reclamo.
        /// </summary>
        [Column("idDepartamento")]
        public long IdDepartamento { get; set; }

        /// <summary>
        /// Obtiene o establece la fecha y hora de registro de la orden de proceso.
        /// Timestamp de cuando se configuró este paso en el sistema.
        /// </summary>
        [Column("fechaRegistro")]
        public DateTime FechaRegistro { get; set; }

        /// <summary>
        /// Obtiene o establece el indicador de si la orden de proceso está activa.
        /// Permite habilitar o deshabilitar pasos del workflow sin eliminarlos del sistema.
        /// </summary>
        [Column("activo")]
        public bool Activo { get; set; }
    }
}
