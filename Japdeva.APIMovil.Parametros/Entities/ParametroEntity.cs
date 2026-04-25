using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Japdeva.APIMovil.Parametros.Entities
{
    /// <summary>
    /// Representa la entidad de parámetro utilizada para gestionar los parámetros del sistema.
    /// </summary>
    [Table("Tbl_Parametro")]
    public class ParametroEntity
    {
        /// <summary>
        /// Obtiene o establece el identificador único del parámetro.
        /// </summary>
        [Column("id")]
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Obtiene o establece el nombre del parámetro.
        /// </summary>
        [Column("nombre")]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece el valor del parámetro.
        /// </summary>
        [Column("valor")]
        public string Valor { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece la descripción del parámetro.
        /// </summary>
        [Column("descripcion")]
        [MaxLength(255)]
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece el identificador del usuario interno asociado al parámetro.
        /// </summary>
        [Column("idUsuarioInterno")]
        public long? IdUsuarioInterno { get; set; }

        /// <summary>
        /// Obtiene o establece la fecha y hora de registro del parámetro.
        /// </summary>
        [Column("fechaRegistro")]
        public DateTime FechaRegistro { get; set; }

        /// <summary>
        /// Obtiene o establece la fecha y hora de la última edición del parámetro.
        /// </summary>
        [Column("fechaEdicion")]
        public DateTime? FechaEdicion { get; set; }

        /// <summary>
        /// Obtiene o establece si el parámetro está activo.
        /// </summary>
        [Column("activo")]
        public bool Activo { get; set; }
    }
}
