using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Japdeva.APIMovil.Usuarios.Entities
{
    /// <summary>
    /// Entidad que representa la asignación de un usuario a un departamento.
    /// </summary>
    [Table("Tbl_Departamento_Usuario")]
    public class DepartamentoUsuarioEntity
    {
        /// <summary>
        /// Obtiene o establece el identificador único del registro.
        /// </summary>
        [Key]
        [Column("id")]
        public long Id { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del usuario asignado al departamento.
        /// </summary>
        [Required]
        [Column("idUsuario")]
        public int IdUsuario { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del departamento.
        /// </summary>
        [Required]
        [Column("idDepartamento")]
        public int IdDepartamento { get; set; }

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
