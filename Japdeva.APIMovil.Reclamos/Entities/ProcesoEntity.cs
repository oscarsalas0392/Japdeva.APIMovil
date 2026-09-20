using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Japdeva.APIMovil.Reclamos.Entities
{
    /// <summary>
    /// Entidad que representa un tipo de proceso en el sistema de reclamos.
    /// Permite distinguir entre el proceso de Reclamo y el proceso de Apelación,
    /// siendo referenciada por los niveles de proceso para determinar a qué flujo pertenecen.
    /// </summary>
    [Table("Tbl_Proceso")]
    public class ProcesoEntity
    {
        /// <summary>
        /// Obtiene o establece el identificador único del proceso.
        /// </summary>
        [Column("id")]
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Obtiene o establece el nombre del proceso.
        /// </summary>
        [Column("nombre")]
        [MaxLength(100)]
        [MinLength(1)]
        [Required]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece la descripción del proceso.
        /// </summary>
        [Column("descripcion")]
        [MaxLength(500)]
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece la fecha y hora de registro del proceso.
        /// </summary>
        [Column("fechaRegistro")]
        public DateTime FechaRegistro { get; set; }

        /// <summary>
        /// Obtiene o establece el indicador de si el proceso está activo.
        /// </summary>
        [Column("activo")]
        public bool Activo { get; set; }

    }
}
