using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Japdeva.APIMovil.Reclamos.Entities
{
    /// <summary>
    /// Entidad que representa un reclamo en el sistema.
    /// Almacena la información principal de los reclamos creados por usuarios externos,
    /// incluyendo su estado actual y datos de auditoría.
    /// </summary>
    [Table("Tbl_Reclamo")]
    public class ReclamoEntity
    {
        /// <summary>
        /// Obtiene o establece el identificador único del reclamo.
        /// </summary>
        [Column("id")]
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// Obtiene o establece el título descriptivo del reclamo.
        /// </summary>
        [Column("titulo")]
        [MaxLength(200)]
        [MinLength(1)]
        [Required]
        public string Titulo { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece la descripción detallada del reclamo.
        /// </summary>
        [Column("descripcion")]
        [MaxLength(1000)]
        [MinLength(1)]
        [Required]
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece el identificador del estado actual del reclamo.
        /// </summary>
        [Column("idEstadoReclamo")]
        public int IdEstadoReclamo { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del usuario externo que creó el reclamo.
        /// </summary>
        [Column("idUsuarioExterno")]
        public long IdUsuarioExterno { get; set; }

        /// <summary>
        /// Obtiene o establece la fecha y hora de registro del reclamo.
        /// </summary>
        [Column("fechaRegistro")]
        public DateTime FechaRegistro { get; set; }

        /// <summary>
        /// Obtiene o establece el indicador de si el reclamo está activo en el sistema.
        /// </summary>
        [Column("idDepartamentoActual")]
        public long IdDepartamentoActual { get; set; }

        /// <summary>
        /// Obtiene o establece la descripción del departamento actual asociado al reclamo.
        /// </summary>
        [Column("descripcionDepartamentoActual")]
        public string DescripcionDepartamentoActual { get; set; } = string.Empty;
    }
}
