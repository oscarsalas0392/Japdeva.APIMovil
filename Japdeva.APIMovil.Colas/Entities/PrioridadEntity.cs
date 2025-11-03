using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Japdeva.APIMovil.Colas.Entities
{
    /// <summary>
    /// Entidad que representa la prioridad de un mensaje en cola para persistencia en base de datos.
    /// </summary>
    [Table("Tbl_Prioridad")]
    public class PrioridadEntity
    {
        /// <summary>
        /// Obtiene o establece el identificador único del mensaje en cola.
        /// </summary>
        [Key]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// Obtiene o establece el nombre descriptivo del estado.
        /// </summary>
        [Required]
        [MaxLength(50)]
        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece la descripción del estado.
        /// </summary>
        [MaxLength(200)]
        [Column("descripcion")]
        public string? Descripcion { get; set; }
        /// <summary>
        /// Obtiene o establece si el estado está activo.
        /// </summary>
        [Column("activo")]
        public bool Activo { get; set; } = true;

        /// <summary>
        /// Obtiene o establece la fecha de creación del registro.
        /// </summary>
        [Column("fechaRegistro")]
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    }
}