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
        /// Obtiene o establece el nombre del usuario.
        /// </summary>
        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

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
        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; }

        /// <summary>
        /// Obtiene o establece la fecha de última actualización del usuario.
        /// </summary>
        [Column("fecha_actualizacion")]
        public DateTime? FechaActualizacion { get; set; }

        /// <summary>
        /// Obtiene o establece un valor que indica si el usuario está activo.
        /// </summary>
        [Column("activo")]
        public bool Activo { get; set; }
    }
}