using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Japdeva.APIMovil.Usuarios.Entities
{
    /// <summary>
    /// Representa un usuario en el sistema.
    /// </summary>
    /// <remarks>
    /// Esta clase contiene la información básica de un usuario.
    /// </remarks>
    [Table("Tbl_Usuario")]
    public class UsuarioEntity
    {
        /// <summary>
        /// Obtiene o establece el identificador único del usuario.
        /// </summary>
        [Column("id")]
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Obtiene o establece la identificacion del usuario.
        /// </summary>
        [Column("identificacion")]
        public string Identificacion { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece el apellidos del usuario.
        /// </summary>
        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece los apelidos del usuario.
        /// </summary>
        [Column("apellidos")]
        public string Apellidos { get; set; } = string.Empty;


        /// <summary>
        /// Obtiene o establece la contraseña del usuario.
        /// </summary>
        [Column("contrasena")]
        public string Contrasena { get; set; } = string.Empty;
        /// <summary>
        /// Obtiene o establece el correo electrónico del usuario.
        /// </summary>
        [Column("correo")]
        public string Correo { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece el teléfono del usuario.
        /// </summary>
        [Column("telefono")]
        public string Telefono { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece la fecha de creación del usuario.
        /// </summary>
        [Column("fechaRegistro")]
        public DateTime FechaCreacion { get; set; }

        /// <summary>
        /// Obtiene o establece la fecha de última actualización del usuario.
        /// </summary>
        [Column("fechaEdicion")]
        public DateTime? FechaActualizacion { get; set; }

        /// <summary>
        /// Obtiene o establece un valor que indica si el usuario está activo.
        /// </summary>
        [Column("activo")]
        public bool Activo { get; set; }
    }
}