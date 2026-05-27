using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Japdeva.APIMovil.Parametros.Entities
{
    /// <summary>
    /// Representa la entidad de opción de pantalla para controlar la visibilidad de elementos de la interfaz de usuario.
    /// </summary>
    [Table("Tbl_OpcionPantalla")]
    public class OpcionPantallaEntity
    {
        /// <summary>
        /// Obtiene o establece el identificador único de la opción de pantalla.
        /// </summary>
        [Column("id")]
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Obtiene o establece el nombre clave de la opción de pantalla.
        /// </summary>
        [Column("nombre")]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece la descripción de la opción de pantalla.
        /// </summary>
        [Column("descripcion")]
        [MaxLength(200)]
        public string? Descripcion { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del usuario interno que registró la opción.
        /// </summary>
        [Column("idUsuarioInterno")]
        public long? IdUsuarioInterno { get; set; }

        /// <summary>
        /// Obtiene o establece la fecha y hora de registro.
        /// </summary>
        [Column("fechaRegistro")]
        public DateTime FechaRegistro { get; set; }

        /// <summary>
        /// Obtiene o establece la fecha y hora de la última edición.
        /// </summary>
        [Column("fechaEdicion")]
        public DateTime? FechaEdicion { get; set; }

        /// <summary>
        /// Obtiene o establece si la opción de pantalla está activa.
        /// </summary>
        [Column("activo")]
        public bool Activo { get; set; }
    }
}
