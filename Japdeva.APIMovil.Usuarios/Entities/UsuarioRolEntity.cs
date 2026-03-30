using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Japdeva.APIMovil.Usuarios.Entities
{
    /// <summary>
    /// Entidad que representa la asignación de un rol a un usuario.
    /// </summary>
    [Table("Tbl_Usuario_Rol")]
    public class UsuarioRolEntity
    {
        /// <summary>
        /// Obtiene o establece el identificador único del registro.
        /// </summary>
        [Key]
        [Column("id")]
        public long Id { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del usuario al que se asigna el rol.
        /// </summary>
        [Required]
        [Column("idUsuario")]
        public int IdUsuario { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del rol asignado.
        /// </summary>
        [Required]
        [Column("idRol")]
        public int IdRol { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del usuario administrador que realizó la asignación.
        /// </summary>
        [Required]
        [Column("idUsuarioAdministrador")]
        public int IdUsuarioAdministrador { get; set; }

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
        /// Obtiene o establece si la asignación está activa.
        /// </summary>
        [Column("activo")]
        public bool Activo { get; set; } = true;
    }
}
