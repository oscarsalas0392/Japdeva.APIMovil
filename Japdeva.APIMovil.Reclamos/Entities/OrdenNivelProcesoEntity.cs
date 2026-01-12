using System.ComponentModel.DataAnnotations.Schema;

namespace Japdeva.APIMovil.Reclamos.Entities
{
    /// <summary>
    /// Representa la relación jerárquica entre niveles de proceso, 
    /// identificando el nivel superior e inferior en la estructura de orden.
    /// </summary>

    [Table("Tbl_OrdenNivelProceso")]
    public class OrdenNivelProcesoEntity
    {
        /// <summary>
        /// Identificador único de la relación jerárquica entre niveles de proceso.
        /// </summary>
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// Identificador del nivel superior en la jerarquía del proceso.
        /// </summary>
        [Column("idNivelSuperior")]
        public int IdNivelSuperior { get; set; }

        /// <summary>
        /// Identificador del nivel inferior en la jerarquía del proceso.
        /// </summary>
        [Column("idNivelInferior")]
        public int IdNivelInferior { get; set; }

        /// <summary>
        /// Indica si el proceso es una devolución a niveles anteriores en la jerarquía.
        /// </summary>
        [Column("devolucionNivel")]
        public bool DevolucionNivel { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del usuario interno que configuró esta orden de proceso.
        /// Permite auditoría de quién estableció la configuración del workflow.
        /// </summary>

        [Column("idUsuarioInterno")]
        public long IdUsuarioInterno { get; set; }

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
