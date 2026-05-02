using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Japdeva.APIMovil.Usuarios.Entities
{
    /// <summary>
    /// Entidad que representa un usuario registrado en el sistema.
    /// </summary>
    [Table("Tbl_Usuario")]
    public class UsuarioEntity
    {
        /// <summary>
        /// Obtiene o establece el identificador único del usuario.
        /// </summary>
        [Key]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// Obtiene o establece el número de identificación (cédula) del usuario.
        /// </summary>
        [Required]
        [MaxLength(50)]
        [Column("identificacion")]
        public string Identificacion { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece el identificador del tipo de cédula del usuario.
        /// </summary>
        [Required]
        [Column("idTipoCedula")]
        public int IdTipoCedula { get; set; }

        /// <summary>
        /// Obtiene o establece el nombre del usuario.
        /// </summary>
        [Required]
        [MaxLength(200)]
        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece los apellidos del usuario.
        /// </summary>
        [Required]
        [MaxLength(200)]
        [Column("apellidos")]
        public string Apellidos { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece el correo electrónico del usuario.
        /// </summary>
        [Required]
        [MaxLength(200)]
        [Column("correo")]
        public string Correo { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece la contraseña cifrada del usuario.
        /// </summary>
        [Required]
        [Column("contrasena")]
        public string Contrasena { get; set; } = string.Empty;

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
        /// Obtiene o establece la fecha de expiración de la contraseña temporal.
        /// Null indica que la contraseña es permanente y no tiene vencimiento.
        /// </summary>
        [Column("fechaExpiracionContrasena")]
        public DateTime? FechaExpiracionContrasena { get; set; }

        /// <summary>
        /// Obtiene o establece el número de teléfono del usuario.
        /// </summary>
        [MaxLength(20)]
        [Column("telefono")]
        public string? Telefono { get; set; }

        /// <summary>
        /// Obtiene o establece la fecha de nacimiento del usuario.
        /// </summary>
        [Column("fechaNacimiento")]
        public DateTime? FechaNacimiento { get; set; }

        /// <summary>
        /// Obtiene o establece si el usuario está activo en el sistema.
        /// </summary>
        [Column("activo")]
        public bool Activo { get; set; } = true;
    }
}
