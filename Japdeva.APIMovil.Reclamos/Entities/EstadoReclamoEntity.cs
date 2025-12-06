using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Japdeva.APIMovil.Reclamos.Entities
{
    /// <summary>
    /// Entidad que representa los posibles estados de un reclamo en el sistema.
    /// Define el catálogo de estados por los que puede pasar un reclamo durante su ciclo de vida.
    /// </summary>
    [Table("Tbl_EstadoReclamo")]
    public class EstadoReclamoEntity
    {
        /// <summary>
        /// Obtiene o establece el identificador único del estado de reclamo.
        /// </summary>
        [Column("id")]
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Obtiene o establece la descripción del estado de reclamo.
        /// Nombre descriptivo del estado como Pendiente, En Proceso, Resuelto, Cerrado.
        /// </summary>
        [Column("descripcion")]
        [MaxLength(100)]
        [MinLength(1)]
        [Required]
        public string Descripcion { get; set; } = string.Empty;

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
