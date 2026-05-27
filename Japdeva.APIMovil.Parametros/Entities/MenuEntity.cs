using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Japdeva.APIMovil.Parametros.Entities
{
    /// <summary>
    /// Representa la entidad de menú utilizada para la gestión de menús en el sistema.
    /// </summary>
    [Table("Tbl_Menu")]
    public class MenuEntity
    {
        /// <summary>
        /// Obtiene o establece el identificador único del perfil.
        /// </summary>

        [Column("id")]
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Obtiene o establece la descripción del perfil.
        /// </summary>

        [Column("descripcion")]
        [MaxLength(100)]
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece la ruta de navegación del menú.
        /// </summary>
        [Column("ruta")]
        [MaxLength(200)]
        public string? Ruta { get; set; }

        /// <summary>
        /// Obtiene o establece el nombre del ícono asociado al menú.
        /// </summary>
        [Column("icono")]
        [MaxLength(100)]
        public string? Icono { get; set; }

        /// <summary>
        /// Obtiene o establece el orden de visualización del menú.
        /// </summary>
        [Column("orden")]
        public int Orden { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del menú padre, si es un submenú.
        /// </summary>
        [Column("idPadre")]
        public int? IdPadre { get; set; }

        /// <summary>
        /// Obtiene o establece si el menú debe mostrarse en la interfaz.
        /// </summary>
        [Column("mostrar")]
        public bool Mostrar { get; set; }

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
