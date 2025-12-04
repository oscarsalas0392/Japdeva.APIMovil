using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Japdeva.APIMovil.Colas.Entities
{
    /// <summary>
    /// Entidad que representa los estados posibles de los mensajes en cola.
    /// </summary>
    [Table("Tbl_EstadoMensaje")]
    public class EstadoMensajeEntity
    {
        /// <summary>
        /// Obtiene o establece el identificador único del estado.
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
        public string Descripcion { get; set; } = string.Empty;

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