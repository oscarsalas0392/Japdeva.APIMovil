using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Japdeva.APIMovil.Usuarios.Entities
{
    /// <summary>
    /// Entidad que representa un tipo de cédula de identificación.
    /// </summary>
    [Table("Tbl_TipoCedula")]
    public class TipoCedulaEntity
    {
        /// <summary>
        /// Obtiene o establece el identificador único del tipo de cédula.
        /// </summary>
        [Key]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// Obtiene o establece el tipo de cédula (Física, Jurídica, etc.).
        /// </summary>
        [Required]
        [MaxLength(100)]
        [Column("tipo")]
        public string Tipo { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece el formato de validación de la cédula.
        /// </summary>
        [MaxLength(100)]
        [Column("formato")]
        public string Formato { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece la fecha de creación del registro.
        /// </summary>
        [Column("fechaRegistro")]
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Obtiene o establece la fecha de última modificación del registro.
        /// </summary>
        [Column("fechaEdicion")]
        public DateTime? FechaEdicion { get; set; }

        /// <summary>
        /// Obtiene o establece si el tipo de cédula está activo.
        /// </summary>
        [Column("activo")]
        public bool Activo { get; set; } = true;
    }
}
