using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Japdeva.APIMovil.Parametros.Entities
{
    /// <summary>
    /// Representa la entidad que asocia un menú con un perfil, incluyendo información de usuario, fechas y estado de activación.
    /// </summary>
     
    [Table("Tbl_MenuPerfil")]
    public class MenuPerfilEntity
    { 
        /// <summary>
        /// Obtiene o establece el identificador único del perfil.
        /// </summary>

        [Column("id")]
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del menú asociado al perfil.
        /// </summary>
        [Column("idMenu")]
        public int IdMenu { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del perfil asociado.
        /// </summary>
        [Column("idPerfil")]
        public int IdPerfil { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del usuario interno asociado al perfil.
        /// </summary>
        [Column("idUsuarioInterno")]
        public long? IdUsuarioInterno { get; set; }

        /// <summary>
        /// Obtiene o establece la fecha y hora de registro del detalle.
        /// </summary>
        [Column("fechaRegistro")]
        public DateTime FechaRegistro { get; set; }

        /// <summary>
        /// Obtiene o establece la fecha y hora de la última edición del detalle.
        /// </summary>
        [Column("fechaEdicion")]
        public DateTime? FechaEdicion { get; set; }

        /// <summary>
        /// Obtiene o establece si el perfil está activo.
        /// </summary>

        [Column("activo")]
        public bool Activo { get; set; }
    }
}
