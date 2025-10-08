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
    [Table("Usuario")]
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
    }
}