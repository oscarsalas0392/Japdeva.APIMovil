using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Japdeva.APIMovil.Usuarios.Entities
{
    /// <summary>
    /// Entidad que representa un rol de usuario en el sistema.
    /// </summary>
    [Table("Tbl_Rol")]
    public class RolEntity
    {
        /// <summary>
        /// Obtiene o establece el identificador único del rol.
        /// </summary>
        [Key]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// Obtiene o establece la descripción del rol.
        /// </summary>
        [Required]
        [MaxLength(200)]
        [Column("descripcion")]
        public string Descripcion { get; set; } = string.Empty;

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
        /// Obtiene o establece si el rol está activo.
        /// </summary>
        [Column("activo")]
        public bool Activo { get; set; } = true;
    }
}
