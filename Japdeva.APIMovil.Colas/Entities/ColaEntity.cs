using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Japdeva.APIMovil.Colas.Entities
{
    /// <summary>
    /// Entidad que representa una cola para el manejo de mensajes en el sistema.
    /// </summary>
    [Table("queue")]
    public class ColaEntity
    {
        /// <summary>
        /// Obtiene o establece el identificador único de la cola.
        /// </summary>
        [Key]
        [Column("id")]
        public Int64 Id { get; set; }

        /// <summary>
        /// Obtiene o establece el nombre de la cola.
        /// </summary>
        [Column("nombre")]
        [Required]
        [MaxLength(255)]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece un valor que indica si la cola está activa.
        /// </summary>
        [Column("activo")]
        public bool Activo { get; set; } = true;

        /// <summary>
        /// Obtiene o establece la fecha y hora de creación de la cola.
        /// </summary>
        [Column("fechaRegistro")]
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Obtiene o establece la fecha y hora de la última actualización de la cola.
        /// </summary>
        [Column("fechaEdicion")]
        public DateTime? FechaEdicion{ get; set; }
    }

}